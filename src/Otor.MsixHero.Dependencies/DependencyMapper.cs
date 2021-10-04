// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Dependencies.Domain;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Dependencies
{
    public class DependencyMapper : IDependencyMapper
    {
        private readonly ISelfElevationProxyProvider<IAppxPackageQuery> packageQuery;

        public DependencyMapper(ISelfElevationProxyProvider<IAppxPackageQuery> packageQuery)
        {
            this.packageQuery = packageQuery;
        }

        public async Task<DependencyGraph> GetGraph(string initialPackage, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            progress?.Report(new ProgressData(0, $"Reading {Path.GetFileName(initialPackage)}..."));

            var reader = new AppxManifestReader();
            using var fileReader = FileReaderFactory.CreateFileReader(initialPackage);
            var pkg = await reader.Read(fileReader, cancellationToken).ConfigureAwait(false);
            return await this.GetGraph(pkg, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<DependencyGraph> GetGraph(AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var dependencyGraph = new DependencyGraph();
            
            var list = await this.GetConsideredPackages(package, cancellationToken, progress).ConfigureAwait(false);
            
            var packages = this.GetPackageDependencies(0, list).ToList();
            var operatingSystems = this.GetOperatingSystemDependencies(packages.Count, list).ToList();
            
            var unresolvedDependencies = new List<MissingPackageGraphElement>();

            var id = packages.Count + operatingSystems.Count;
            foreach (var pkg in packages)
            {
                foreach (var packageDependency in pkg.Package.PackageDependencies)
                {
                    var findDependency = packages.FirstOrDefault(depPkg => depPkg.Package.Name == packageDependency.Name && depPkg.Package.Publisher == packageDependency.Publisher);
                    if (findDependency != null)
                    {
                        dependencyGraph.Relations.Add(new Relation(findDependency, packageDependency.Version + "˄", pkg));
                    }
                    else
                    {
                        var findUnresolved = unresolvedDependencies.FirstOrDefault(unresolved => unresolved.PackageName == packageDependency.Name);
                        if (findUnresolved == null)
                        {
                            id++;
                            findUnresolved = new MissingPackageGraphElement(id, packageDependency.Name);
                            unresolvedDependencies.Add(findUnresolved);
                            dependencyGraph.Elements.Add(findUnresolved);
                        }

                        dependencyGraph.Relations.Add(new Relation(findUnresolved, packageDependency.Version + "˄", pkg));
                    }
                }

                foreach (var systemDependency in pkg.Package.OperatingSystemDependencies)
                {
                    var findSystem = operatingSystems.First(os => os.OperatingSystem == GetOsFamily(systemDependency));
                    dependencyGraph.Relations.Add(new Relation(findSystem, findSystem.MaxRequiredVersion + "˄", pkg));
                }

                foreach (var mainPackage in pkg.Package.MainPackages)
                {
                    var findParent = packages.FirstOrDefault(depPkg => depPkg.Package.Name == mainPackage.Name);
                    if (findParent != null)
                    {
                        dependencyGraph.Relations.Add(new Relation(findParent, "main package", pkg));
                    }
                    else
                    {
                        var findUnresolved = unresolvedDependencies.FirstOrDefault(unresolved => unresolved.PackageName == mainPackage.Name);
                        if (findUnresolved == null)
                        {
                            id++;
                            findUnresolved = new MissingPackageGraphElement(id, mainPackage.Name);
                            unresolvedDependencies.Add(findUnresolved);
                            dependencyGraph.Elements.Add(findUnresolved);
                        }

                        dependencyGraph.Relations.Add(new Relation(findUnresolved, "main package", pkg));
                    }
                }
            }

            foreach (var pkg in packages)
            {
                dependencyGraph.Elements.Add(pkg);
            }

            foreach (var os in operatingSystems)
            {
                dependencyGraph.Elements.Add(os);
            }

            return dependencyGraph;
        }

        private IEnumerable<PackageGraphElement> GetPackageDependencies(int startNumberingAt, IEnumerable<AppxPackage> packages)
        {
            var id = startNumberingAt;
            
            foreach (var pkg in packages)
            {   
                if (id == 0)
                {
                    yield return new RootGraphElement(pkg);
                }
                else
                {
                    yield return new PackageGraphElement(id, pkg);
                }
                
                id++;
            }
        }

        private static string GetOsFamily(AppxOperatingSystemDependency system)
        {
            return GetOsFamily(system.Minimum);
        }

        private static string GetOsFamily(AppxTargetOperatingSystem system)
        {
            return (system.NativeFamilyName.Contains("windows", StringComparison.OrdinalIgnoreCase) ? "Windows" : system.NativeFamilyName) + "-" + Version.Parse(system.TechnicalVersion).ToString(2);
        }

        private IEnumerable<OperatingSystemGraphElement> GetOperatingSystemDependencies(int startNumberingAt, IEnumerable<AppxPackage> packages)
        {
            var id = startNumberingAt;
            foreach (var system in packages
                .SelectMany(pkg => pkg.OperatingSystemDependencies)
                .GroupBy(GetOsFamily))
            {
                id++;
                var highestMinimumRequiredSystemVersion = system.Select(item => Version.Parse(item.Minimum.TechnicalVersion)).Max();
                var highestMinimumRequiresSystem = system.First(s => Version.Parse(s.Minimum.TechnicalVersion) == highestMinimumRequiredSystemVersion).Minimum;
                
                var family = system.Key;
                yield return new OperatingSystemGraphElement(id, family)
                {
                    MaxRequiredCaption = string.IsNullOrEmpty(highestMinimumRequiresSystem.MarketingCodename) ? highestMinimumRequiresSystem.Name : highestMinimumRequiresSystem.Name + Environment.NewLine + highestMinimumRequiresSystem.MarketingCodename,
                    MaxRequiredVersion = Version.Parse(highestMinimumRequiresSystem.TechnicalVersion)
                };
            }
        }
        
        private async Task<IList<AppxPackage>> GetConsideredPackages(AppxPackage startPackage, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var progressForGettingPackages = new RangeProgress(progress, 0, 70);
            var progressForGettingAddOns = new RangeProgress(progress, 70, 90);
            var progressForCalculation = new RangeProgress(progress, 90, 100);
            
            var manager = await this.packageQuery.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            var allPackages = await manager.GetInstalledPackages(PackageFindMode.Auto, cancellationToken, progressForGettingPackages).ConfigureAwait(false);
            var consideredPackages = new List<AppxPackage> { startPackage };
            var addOnPackages = new List<AppxPackage>();

            var manifestReader = new AppxManifestReader();

            progressForGettingAddOns.Report(new ProgressData(0, "Reading optional packages..."));
            foreach (var addOnPackage in allPackages.Where(installedPackage => installedPackage.IsOptional))
            {
                using var fileReader = FileReaderFactory.CreateFileReader(addOnPackage.ManifestLocation);
                addOnPackages.Add(await manifestReader.Read(fileReader, false, cancellationToken).ConfigureAwait(false));
            }

            progressForCalculation.Report(new ProgressData(0, "Reading relations..."));
            for (var i = 0; i < consideredPackages.Count; i++)
            {
                var currentPkg = consideredPackages[i];

                var matchingAddOns = addOnPackages.Where(addOnPackage => addOnPackage.MainPackages.Any(dep => dep.Name == currentPkg.Name));
                foreach (var matchingAddOn in matchingAddOns)
                {
                    if (consideredPackages.Any(existing => existing.Publisher == matchingAddOn.Publisher && existing.Name == matchingAddOn.Name))
                    {
                        // we have already processes this package
                        continue;
                    }

                    consideredPackages.Add(matchingAddOn);
                }
                
                foreach (var dependency in currentPkg.PackageDependencies)
                {
                    if (consideredPackages.Any(existing => existing.Publisher == dependency.Publisher && existing.Name == dependency.Name))
                    {
                        // we have already processes this package
                        continue;
                    }
                    
                    var candidate = allPackages.FirstOrDefault(installedPackage => installedPackage.Name == dependency.Name && installedPackage.Publisher == dependency.Publisher && installedPackage.Version >= Version.Parse(dependency.Version));

                    if (candidate != null)
                    {
                        using var fileReader = FileReaderFactory.CreateFileReader(candidate.ManifestLocation);
                        consideredPackages.Add(await manifestReader.Read(fileReader, false, cancellationToken).ConfigureAwait(false));
                    }
                }

                foreach (var dependency in currentPkg.MainPackages)
                {
                    if (consideredPackages.Any(existing => existing.Name == dependency.Name))
                    {
                        // we have already processes this package
                        continue;
                    }

                    var candidate = allPackages.FirstOrDefault(installedPackage => installedPackage.Name == dependency.Name);

                    if (candidate != null)
                    {
                        using var fileReader = FileReaderFactory.CreateFileReader(candidate.ManifestLocation);
                        consideredPackages.Add(await manifestReader.Read(fileReader, false, cancellationToken).ConfigureAwait(false));
                    }
                }
            }
            
            return consideredPackages;
        }
    }
}