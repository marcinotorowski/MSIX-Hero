using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Branding;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Registry.Converter;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Appx.Packaging.ManifestCreator
{
    public class AppxManifestCreator : IAppxManifestCreator
    {
        public async IAsyncEnumerable<CreatedItem> CreateManifestForDirectory(
            DirectoryInfo sourceDirectory,
            AppxManifestCreatorOptions options = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default,
            IProgress<Progress> progress = null)
        {
            options ??= AppxManifestCreatorOptions.Default;
            
            // Add logo to assets if there is nothing
            if (options.CreateLogo)
            {
                CreatedItem logo = default;
                foreach (var entryPoint in options.EntryPoints ?? Enumerable.Empty<string>())
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var entryPointPath = new FileInfo(Path.Combine(sourceDirectory.FullName, entryPoint));
                        if (entryPointPath.Exists)
                        {
                            logo = await this.CreateLogo(entryPointPath).ConfigureAwait(false);
                        }
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }

                    if (!default(CreatedItem).Equals(logo))
                    {
                        break;
                    }
                }

                if (default(CreatedItem).Equals(logo))
                {
                    logo = await this.CreateDefaultLogo(cancellationToken).ConfigureAwait(false);
                }

                yield return logo;
            }
            
            if (options.RegistryFile?.Exists == true)
            {
                await foreach (var registryItem in this.CreateRegistryEntries(options.RegistryFile, cancellationToken).ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!registryItem.Equals(default(CreatedItem)))
                    {
                        yield return registryItem;
                    }
                }
            }

            // The actual part - create the manifest
            var modPackageTemplate = GetBundledResourcePath("ModificationPackage.AppxManifest.xml");
            await using var openTemplate = File.OpenRead(modPackageTemplate);
            var xml = await XDocument.LoadAsync(openTemplate, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            var entryPoints = options.EntryPoints;

            if (entryPoints?.Any() != true)
            {
                var getExeFiles = await this.GetEntryPointCandidates(sourceDirectory, cancellationToken).ConfigureAwait(false);
                entryPoints = getExeFiles.ToArray();
            }

            if (options.EntryPoints?.Any() == true)
            {
                foreach (var entryPoint in options.EntryPoints)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ExceptionGuard.Guard(() =>
                    {
                        var fvi = FileVersionInfo.GetVersionInfo(Path.Combine(sourceDirectory.FullName, entryPoint));

                        if (options.Version == null)
                        {
                            options.Version = Version.TryParse(fvi.ProductVersion ?? fvi.FileVersion ?? "#", out var v) ? v : null;
                        }

                        if (string.IsNullOrEmpty(options.PackageName) && !string.IsNullOrEmpty(Sanitize(fvi.ProductName)))
                        {
                            options.PackageName = Sanitize(fvi.ProductName, "ProductName")?.Trim();
                        }

                        if (string.IsNullOrEmpty(options.PackageDisplayName) && !string.IsNullOrEmpty(fvi.ProductName))
                        {
                            options.PackageDisplayName = fvi.ProductName?.Trim();
                        }

                        if (string.IsNullOrEmpty(options.PublisherName) && !string.IsNullOrEmpty(Sanitize(fvi.CompanyName)))
                        {
                            options.PublisherName = "CN=" + Sanitize(fvi.CompanyName, "CompanyName");
                        }

                        if (string.IsNullOrEmpty(options.PublisherDisplayName) && !string.IsNullOrEmpty(Sanitize(fvi.CompanyName)))
                        {
                            options.PublisherDisplayName = fvi.CompanyName?.Trim();
                        }
                    });

                    if (options.Version != null &&
                        !string.IsNullOrEmpty(options.PublisherName) &&
                        !string.IsNullOrEmpty(options.PublisherDisplayName) &&
                        !string.IsNullOrEmpty(options.PackageName) &&
                        !string.IsNullOrEmpty(options.PackageDisplayName))
                    {
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(options.PackageDisplayName))
            {
                options.PackageDisplayName = "MyPackage";
            }

            if (string.IsNullOrEmpty(options.PackageName))
            {
                options.PackageName = "MyPackage";
            }

            if (string.IsNullOrEmpty(options.PublisherName))
            {
                options.PublisherName = "CN=Publisher";
            }

            if (string.IsNullOrEmpty(options.PublisherDisplayName))
            {
                options.PublisherDisplayName = "Publisher";
            }

            if (options.Version == null)
            {
                options.Version = new Version(1, 0, 0);
            }

            this.AdjustManifest(xml, options, sourceDirectory, entryPoints);
            var manifestContent = xml.ToString(SaveOptions.OmitDuplicateNamespaces);
            
            var manifestFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(FileConstants.AppxManifestFile) + "-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".xml");
            await File.WriteAllTextAsync(manifestFilePath, manifestContent, Encoding.UTF8, cancellationToken);

            yield return CreatedItem.CreateManifest(manifestFilePath);
        }

        private static string Sanitize(string input, string defaultIfNull = null)
        {
            var result = Regex.Replace(input, @"[^a-zA-Z0-9\.]+", string.Empty).Trim();
            if (string.IsNullOrEmpty(result))
            {
                return defaultIfNull;
            }

            return result;
        }

        public async Task<AppxManifestCreatorAdviser> AnalyzeDirectory(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
        {
            var result = new AppxManifestCreatorAdviser();

            var manifest = new FileInfo(Path.Combine(directoryInfo.FullName, FileConstants.AppxManifestFile));
            if (manifest.Exists)
            {
                result.Manifest = manifest;
            }

            result.RegistryFiles = directoryInfo.EnumerateFiles("*.reg", SearchOption.AllDirectories).ToArray();
            result.Directory = directoryInfo;
            result.EntryPoints = (await this.GetEntryPointCandidates(directoryInfo, cancellationToken).ConfigureAwait(false)).ToArray();

            if (manifest.Exists)
            {
                var manifestSummary = await AppxManifestSummaryReader.FromManifest(manifest.FullName, AppxManifestSummaryReader.ReadMode.Properties);
                if (manifestSummary.Logo != null)
                {
                    result.Logo = new FileInfo(Path.Combine(directoryInfo.FullName, manifestSummary.Logo.Replace("/", "\\")));
                    if (!result.Logo.Exists)
                    {
                        result.Logo = null;
                    }
                }
            }

            return result;
        }

        public bool CheckIfRegistryConversionPossible(DirectoryInfo sourceDirectory)
        {
            if (!sourceDirectory.Exists)
            {
                return false;
            }

            var registryFiles = sourceDirectory.EnumerateFiles("*.reg", SearchOption.TopDirectoryOnly);
            if (!registryFiles.Any())
            {
                return false;
            }

            var pathAll = Path.Combine(sourceDirectory.FullName, "Registry.dat");
            var pathUser = Path.Combine(sourceDirectory.FullName, "User.dat");
            var pathClassesRoot = Path.Combine(sourceDirectory.FullName, "UserClasses.dat");

            return !File.Exists(pathAll) && !File.Exists(pathUser) && !File.Exists(pathClassesRoot);
        }

        public async Task<IList<string>> GetEntryPointCandidates(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
        {
            var candidates = await this.GetEntryPoints(directoryInfo, cancellationToken).ConfigureAwait(false);
            if (!candidates.Any())
            {
                throw new InvalidOperationException("This folder contains no executable files.");
            }

            var filteredList = candidates.Where(c =>
            {
                var fn = Path.GetFileNameWithoutExtension(c).ToLowerInvariant();
                switch (fn)
                {
                    case "update":
                    case "updater":
                        return false;
                    default:
                        // ReSharper disable once StringLiteralTypo
                        return !fn.StartsWith("unins", StringComparison.OrdinalIgnoreCase);
                }
            }).ToList();

            if (filteredList.Any())
            {
                return filteredList;
            }

            return candidates;
        }

        private async Task<CreatedItem> CreateDefaultLogo(CancellationToken cancellationToken = default)
        {
            var logoSourcePath = Path.Combine(Path.GetTempPath(), "Logo-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".png");
            var bundledLogoPath = GetBundledResourcePath("Logo.png");
            await using var sourceStream = File.OpenRead(bundledLogoPath);
            await using var targetStream = File.OpenWrite(logoSourcePath);
            await sourceStream.CopyToAsync(targetStream, cancellationToken).ConfigureAwait(false);
            return CreatedItem.CreateAsset(logoSourcePath, "Assets/Logo.png");
        }

        private Task<CreatedItem> CreateLogo(FileInfo logoSource)
        {
            string logoSourcePath = Path.Combine(Path.GetTempPath(), "Logo-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".png");
            
            ExceptionGuard.Guard(() =>
            {
                // ReSharper disable once AccessToModifiedClosure
                using var icon = Icon.ExtractAssociatedIcon(logoSource.FullName);
                if (icon == null)
                {
                    return;
                }

                using var bitmap = icon.ToBitmap();
                bitmap.Save(logoSourcePath, ImageFormat.Png);
            });
        
            if (File.Exists(logoSourcePath))
            {
                return Task.FromResult(CreatedItem.CreateAsset(logoSourcePath, "Assets/Logo.png"));
            }

            return Task.FromResult(default(CreatedItem));
        }

        private Task<IList<string>> GetEntryPoints(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                IList<string> allFiles = new List<string>();
                foreach (var file in directoryInfo.EnumerateFiles("*.exe", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fullName = file.FullName;
                    if (fullName.StartsWith(directoryInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        allFiles.Add(Path.GetRelativePath(directoryInfo.FullName, fullName));
                    }
                }

                return (IList<string>)allFiles.OrderBy(fl => fl.Split('\\').Length).ThenBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
            }, cancellationToken);
        }
        
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private void AdjustManifest(XDocument template, AppxManifestCreatorOptions config, DirectoryInfo baseDirectory, string[] entryPoints)
        {
            XNamespace nsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XNamespace nsUap4 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/4";
            XNamespace nsUap6 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/6";
            XNamespace nsRescap = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities";
            XNamespace nsRescap6 = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities/6";
            XNamespace nsBuild = "http://schemas.microsoft.com/developer/appx/2015/build";
            XNamespace defaultNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

            var root = template.Root;
            if (root == null)
            {
                root = new XElement(defaultNamespace + "Package");
                template.Add(root);
            }
            else
            {
                defaultNamespace = root.GetDefaultNamespace();
            }

            if (root.GetPrefixOfNamespace(nsUap) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "uap", nsUap.NamespaceName));
            }

            if (root.GetPrefixOfNamespace(nsUap4) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "uap4", nsUap6.NamespaceName));
            }

            if (root.GetPrefixOfNamespace(nsUap6) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "uap6", nsUap6.NamespaceName));
            }

            if (root.GetPrefixOfNamespace(nsRescap) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "rescap", nsRescap.NamespaceName));
            }

            if (root.GetPrefixOfNamespace(nsRescap6) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "rescap6", nsRescap6.NamespaceName));
            }

            if (root.GetPrefixOfNamespace(nsBuild) == null)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + "build", nsBuild.NamespaceName));
            }

            var package = GetOrCreateNode(template, "Package", defaultNamespace);
            var identity = GetOrCreateNode(package, "Identity", defaultNamespace);

            identity.SetAttributeValue("Name", config.PackageName);
            identity.SetAttributeValue("Publisher", config.PublisherName);
            identity.SetAttributeValue("ProcessorArchitecture", config.PackageArchitecture.ToString("G").ToLowerInvariant());
            
            var major = config.Version.Major;
            var minor = config.Version.Minor;
            var build = config.Version.Build;
            var revision = config.Version.Revision;

            if (major < 0)
            {
                throw new FormatException("Invalid version format, major version is required.");
            }

            if (minor < 0)
            {
                throw new FormatException("Invalid version format, major version is required.");
            }

            if (revision < 0)
            {
                revision = 0;
            }

            if (build < 0)
            {
                build = 0;
            }
            
            identity.SetAttributeValue("Version", new Version(major, minor, build, revision).ToString(4));

            var properties = GetOrCreateNode(package, "Properties", defaultNamespace);
            GetOrCreateNode(properties, "DisplayName", defaultNamespace).Value = config.PackageDisplayName ?? config.PackageName ?? "DisplayName";
            GetOrCreateNode(properties, "Description", defaultNamespace).Value = config.PackageDisplayName ?? config.PackageName ?? "Description";
            GetOrCreateNode(properties, "PublisherDisplayName", defaultNamespace).Value = config.PublisherDisplayName ?? config.PublisherName ?? "Publisher";
            GetOrCreateNode(properties, "Logo", defaultNamespace).Value = "Assets\\Logo.png";
            var applicationsNode = GetOrCreateNode(package, "Applications", defaultNamespace);

            var usedNames = new HashSet<string>();
            foreach (var item in entryPoints)
            {
                var applicationNode = this.CreateApplicationNodeFromExe(baseDirectory, item);
                applicationsNode.Add(applicationNode);

                var idCandidate = Regex.Replace(Path.GetFileNameWithoutExtension(item), "[^a-zA-z0-9_]+", string.Empty);
                if (!usedNames.Add(idCandidate))
                {
                    var index = 1;
                    var baseIdCandidate = idCandidate;
                    while (!usedNames.Add(baseIdCandidate + "_" + index))
                    {
                        index++;
                    }

                    idCandidate = baseIdCandidate + "_" + index;
                }

                applicationNode.SetAttributeValue("Id", idCandidate);
            }

            var capabilities = GetOrCreateNode(package, "Capabilities", defaultNamespace);
            capabilities.Add(new XElement(nsRescap + "Capability", new XAttribute("Name", "runFullTrust")));
            
            var branding = new MsixHeroBrandingInjector();
            branding.Inject(template, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferIncoming);
        }

        private XElement CreateApplicationNodeFromExe(DirectoryInfo directoryInfo, string relativePath)
        {
            XNamespace defaultNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace nsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            var applicationNode = new XElement(defaultNamespace + "Application");
            
            applicationNode.SetAttributeValue("Id", Regex.Replace(Path.GetFileNameWithoutExtension(relativePath), "[^a-zA-z0-9_]+", string.Empty));
            applicationNode.SetAttributeValue("EntryPoint", "Windows.FullTrustApplication");
            applicationNode.SetAttributeValue("Executable", relativePath);

            var visualElements = new XElement(nsUap + "VisualElements");

            var fullFilePath = Path.Combine(directoryInfo.FullName, relativePath);
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"File '{relativePath}' was not found in base directory {directoryInfo.FullName}.");
            }

            var fileInfo = FileVersionInfo.GetVersionInfo(fullFilePath);
            if (!string.IsNullOrEmpty(fileInfo.ProductName))
            {
                visualElements.SetAttributeValue("DisplayName", fileInfo.ProductName);
            }

            visualElements.SetAttributeValue("Square150x150Logo", "Assets/Logo.png");
            visualElements.SetAttributeValue("Square44x44Logo", "Assets/Logo.png");
            visualElements.SetAttributeValue("BackgroundColor", "#333333");
            visualElements.SetAttributeValue("Description", fileInfo.FileDescription ?? fileInfo.ProductName ?? Path.GetFileNameWithoutExtension(fullFilePath));

            applicationNode.Add(visualElements);

            return applicationNode;
        }

        private static XElement GetOrCreateNode(XContainer xmlNode, string name, XNamespace nameSpace = null)
        {
            var node = nameSpace == null ? xmlNode.Descendants(name).FirstOrDefault() : xmlNode.Descendants(nameSpace + name).FirstOrDefault();
            if (node == null)
            {
                node = new XElement(nameSpace + name);
                xmlNode.Add(node);
            }

            return node;
        }

        private static string GetBundledResourcePath(string localName)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", localName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not locale resource {path}.", path);
            }

            return path;
        }

        private async IAsyncEnumerable<CreatedItem> CreateRegistryEntries(FileInfo regFile, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var tempDirectory = Path.GetTempPath();
            var writer = new RegConverter();

            var pathAll = Path.Combine(tempDirectory, "Registry" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".dat");
            var pathUser = Path.Combine(tempDirectory, "User" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".dat");
            var pathClassesRoot = Path.Combine(tempDirectory, "UserClasses" + Guid.NewGuid().ToString("N").Substring(0,10) + ".dat");

            if (await writer.ConvertFromRegToDat(regFile.FullName, pathAll).ConfigureAwait(false))
            {
                yield return CreatedItem.CreateRegistry(pathAll, "Registry.dat");
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (await writer.ConvertFromRegToDat(regFile.FullName, pathUser, RegistryRoot.HKEY_CURRENT_USER).ConfigureAwait(false))
            {
                yield return CreatedItem.CreateRegistry(pathAll, "User.dat");
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (await writer.ConvertFromRegToDat(regFile.FullName, pathClassesRoot, RegistryRoot.HKEY_CLASSES_ROOT).ConfigureAwait(false))
            {
                yield return CreatedItem.CreateRegistry(pathAll, "UserClasses.dat");
            }
        }
    }
}
