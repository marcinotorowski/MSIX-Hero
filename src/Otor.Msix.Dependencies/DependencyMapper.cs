using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.Msix.Dependencies.Domain;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.Msix.Dependencies
{
    public class DependencyMapper : IDependencyMapper
    {
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManager;
        private IList<InstalledPackage> packages;

        public DependencyMapper(ISelfElevationProxyProvider<IAppxPackageManager> packageManager)
        {
            this.packageManager = packageManager;
        }

        public async Task<DependencyGraph> GetGraph(string initialPackage, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            progress?.Report(new ProgressData(0, "Reading manifest..."));

            var reader = new AppxManifestReader();
            if (string.Equals("appxmanifest.xml", Path.GetFileName(initialPackage), StringComparison.OrdinalIgnoreCase))
            {
                using (IAppxFileReader fileReader = new FileInfoFileReaderAdapter(initialPackage))
                {
                    var pkg = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
                    return await this.GetGraph(pkg, cancellationToken, progress).ConfigureAwait(false);
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(initialPackage))
            {
                var pkg = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
                return await this.GetGraph(pkg, cancellationToken, progress).ConfigureAwait(false);
            }
        }

        public async Task<DependencyGraph> GetGraph(AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var dependencies = new List<GraphElement>();
            var relations = new List<Relation>();

            var installedPackages = new Dictionary<string, InstalledPackageGraphElement>();
            var missingPackages = new Dictionary<string, MissingPackageGraphElement>();
            var operatingSystems = new Dictionary<string, OperatingSystemGraphElement>();

            var toProcess = new Queue<AppxPackage>();
            toProcess.Enqueue(package);

            RootGraphElement rootGraphElement = null;
            GraphElement current;
            while (toProcess.TryDequeue(out var file))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!dependencies.Any())
                {
                    rootGraphElement = new RootGraphElement(file);
                    current = rootGraphElement;
                    dependencies.Add(current);
                }
                else
                {
                    current = dependencies.OfType<InstalledPackageGraphElement>().FirstOrDefault(f => f.PackageName == file.Name);
                }
                
                foreach (var item in file.OperatingSystemDependencies ?? Enumerable.Empty<AppxOperatingSystemDependency>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var family = item.Minimum.NativeFamilyName;
                    if (family.Contains("windows", StringComparison.OrdinalIgnoreCase))
                    {
                        family = "Windows";
                    }

                    if (!operatingSystems.TryGetValue(family, out var os))
                    {
                        os = new OperatingSystemGraphElement(dependencies.Count + 1, family);
                        dependencies.Add(os);
                        os.MaxRequiredVersion = Version.Parse(item.Minimum.TechnicalVersion);
                        if (string.IsNullOrEmpty(item.Minimum.MarketingCodename))
                        {
                            os.MaxRequiredCaption = item.Minimum.Name;
                        }
                        else
                        {
                            os.MaxRequiredCaption = item.Minimum.Name + Environment.NewLine + item.Minimum.MarketingCodename;
                        }

                        operatingSystems[family] = os;
                    }
                    else if (os.MaxRequiredVersion < Version.Parse(item.Minimum.TechnicalVersion))
                    {
                        os.MaxRequiredVersion = Version.Parse(item.Minimum.TechnicalVersion);
                        os.MaxRequiredCaption = item.Minimum.Name + System.Environment.NewLine + item.Minimum.MarketingCodename;
                    }

                    relations.Add(new Relation(current, item.Minimum.TechnicalVersion + "+", os));
                }

                var mgr = await this.packageManager.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
                var mainPackages = await mgr.GetModificationPackages(package.FullName, PackageFindMode.Auto, cancellationToken).ConfigureAwait(false);
                foreach (var main in mainPackages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (installedPackages.TryGetValue(main.Name, out var installedMain))
                    {
                        relations.Add(new Relation(installedMain, "modifies", current));
                    }
                    else if (missingPackages.TryGetValue(main.Name, out var missingMain))
                    {
                        relations.Add(new Relation(missingMain, "modifies", current));
                    }
                    else if (rootGraphElement.Package.Name == main.Name)
                    {
                        relations.Add(new Relation(missingMain, "modifies", rootGraphElement));
                    }
                    else
                    {
                        installedMain = new InstalledPackageGraphElement(dependencies.Count + 1, main);
                        installedPackages[main.Name] = installedMain;
                        dependencies.Add(installedMain);
                        relations.Add(new Relation(current, "modifies", installedMain));

                        using (IAppxFileReader subReader = new FileInfoFileReaderAdapter(main.ManifestLocation))
                        {
                            var mr = new AppxManifestReader();
                            toProcess.Enqueue(await mr.Read(subReader, cancellationToken).ConfigureAwait(false));
                        }
                    }
                }

                foreach (var item in file.MainPackages ?? Enumerable.Empty<AppxMainPackageDependency>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var needsLookup = true;
                    if (installedPackages.TryGetValue(item.Name, out var installedMain))
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, "modifies", installedMain));
                    }

                    if (missingPackages.TryGetValue(item.Name, out var missingMain))
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, "modifies", missingMain));
                    }

                    if (rootGraphElement.Package.Name == item.Name)
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, "modifies", rootGraphElement));
                    }

                    if (needsLookup)
                    {
                        if (this.packages == null)
                        {
                            this.packages = await this.GetInstalled(cancellationToken, progress).ConfigureAwait(false);
                        }

                        var find = this.packages.FirstOrDefault(p => p.Name == item.Name);
                        if (find == null)
                        {
                            missingMain = new MissingPackageGraphElement(dependencies.Count + 1, item.Name);
                            missingPackages[item.Name] = missingMain;
                            dependencies.Add(missingMain);
                            relations.Add(new Relation(current, "modifies", missingMain));
                        }
                        else
                        {
                            using (IAppxFileReader subReader = new FileInfoFileReaderAdapter(find.ManifestLocation))
                            {
                                var mr = new AppxManifestReader();
                                toProcess.Enqueue(await mr.Read(subReader).ConfigureAwait(false));
                            }
                                
                            installedMain = new InstalledPackageGraphElement(dependencies.Count + 1, find);
                            installedPackages[item.Name] = installedMain;
                            dependencies.Add(installedMain);
                            relations.Add(new Relation(current, "modifies", installedMain));
                        }
                    }
                }

                foreach (var item in file.PackageDependencies ?? Enumerable.Empty<AppxPackageDependency>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var needsLookup = true;
                    if (installedPackages.TryGetValue(item.Name, out var installed))
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, item.Version + "+", installed));
                    }

                    if (missingPackages.TryGetValue(item.Name, out var missing))
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, item.Version + "+", missing));
                    }

                    if (rootGraphElement.Package.Name == item.Name)
                    {
                        needsLookup = false;
                        relations.Add(new Relation(current, item.Version + "+", rootGraphElement));
                    }

                    if (needsLookup)
                    {
                        if (this.packages == null)
                        {
                            this.packages = await this.GetInstalled(cancellationToken, progress).ConfigureAwait(false);
                        }

                        var myArch = file.ProcessorArchitecture.ToString();
                        var find = this.packages.FirstOrDefault(p => p.Name == item.Name && p.Architecture == myArch) ?? this.packages.FirstOrDefault(p => p.Name == item.Name);
                        if (find == null || find.Version < Version.Parse(item.Version))
                        {
                            missing = new MissingPackageGraphElement(dependencies.Count + 1, item.Name);
                            missingPackages[item.Name] = missing;
                            dependencies.Add(missing);
                            relations.Add(new Relation(current, item.Version + "+", missing));
                        }
                        else
                        {
                            using (IAppxFileReader subReader = new FileInfoFileReaderAdapter(find.ManifestLocation))
                            {
                                var mr = new AppxManifestReader();
                                toProcess.Enqueue(await mr.Read(subReader).ConfigureAwait(false));
                            }

                            installed = new InstalledPackageGraphElement(dependencies.Count + 1, find);
                            installedPackages[item.Name] = installed;
                            dependencies.Add(installed);
                            relations.Add(new Relation(current, item.Version + "+", installed));
                        }
                    }
                }
            }

            return new DependencyGraph
            {
                Elements = dependencies,
                Relations = relations
            };
        }

        private async Task<IList<InstalledPackage>> GetInstalled(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            EventHandler<ProgressData> progressChanged = null;

            if (progress != null)
            {
                progressChanged = (sender, args) =>
                {
                    progress.Report(new ProgressData(args.Progress, "Reading dependencies..."));
                };
            }

            var p = new Progress();
            try
            {
                if (progress != null)
                {
                    p.ProgressChanged += progressChanged;
                }

                var mgr = await this.packageManager.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
                return await mgr.GetInstalledPackages(PackageFindMode.Auto, cancellationToken, p).ConfigureAwait(false);
            }
            finally
            {
                if (progress != null)
                {
                    p.ProgressChanged -= progressChanged;
                }
            }
        }
    }
}