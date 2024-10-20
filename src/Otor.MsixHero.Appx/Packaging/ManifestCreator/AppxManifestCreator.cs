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
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files.Helpers;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Facades;
using Otor.MsixHero.Appx.Reader.Manifest.Entities.Summary;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;

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
                await foreach (var registryItem in this.CreateRegistryEntries(options.RegistryFile).ConfigureAwait(false))
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

            await this.AdjustManifest(xml, options, sourceDirectory, entryPoints).ConfigureAwait(false);
            var manifestContent = xml.ToString(SaveOptions.OmitDuplicateNamespaces);
            
            var manifestFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(AppxFileConstants.AppxManifestFile) + "-" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".xml");
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

            var manifest = new FileInfo(Path.Combine(directoryInfo.FullName, AppxFileConstants.AppxManifestFile));
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
        
        public async Task<IList<string>> GetEntryPointCandidates(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default)
        {
            var candidates = await this.GetEntryPoints(directoryInfo, cancellationToken).ConfigureAwait(false);
            if (!candidates.Any())
            {
                throw new InvalidOperationException(Resources.Localization.Packages_Error_Folder_NoExe);
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
        private async Task AdjustManifest(XDocument template, AppxManifestCreatorOptions config, DirectoryInfo baseDirectory, string[] entryPoints)
        {
            XNamespace nsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            XNamespace nsUap4 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/4";
            XNamespace nsUap6 = "http://schemas.microsoft.com/appx/manifest/uap/windows10/6";
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

            // Add capability
            var addCapability = new AddCapability("runFullTrust");
            var capabilityExecutor = new AddCapabilityExecutor(template);
            await capabilityExecutor.Execute(addCapability).ConfigureAwait(false);

            // Set identity
            var setIdentity = new SetPackageIdentity
            {
                Name = config.PackageName,
                Publisher = config.PublisherName,
                ProcessorArchitecture = config.PackageArchitecture.ToString("G").ToLowerInvariant()
            };

            var major = config.Version.Major;
            var minor = config.Version.Minor;
            var build = config.Version.Build;
            var revision = config.Version.Revision;

            if (major < 0)
            {
                throw new FormatException(Resources.Localization.Packages_Error_InvalidVersion);
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

            setIdentity.Version = new Version(major, minor, build, revision).ToString();
            var executor = new SetPackageIdentityExecutor(template);
            await executor.Execute(setIdentity).ConfigureAwait(false);

            // Add namespaces (legacy)
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
            
            var package = GetOrCreateNode(template, "Package", defaultNamespace);
            
            var properties = GetOrCreateNode(package, "Properties", defaultNamespace);
            GetOrCreateNode(properties, "DisplayName", defaultNamespace).Value = GetNotEmptyValueFromList(config.PackageDisplayName, config.PackageName, "DisplayName");
            GetOrCreateNode(properties, "Description", defaultNamespace).Value = GetNotEmptyValueFromList(config.PackageDisplayName, config.PackageName, "Description");
            GetOrCreateNode(properties, "PublisherDisplayName", defaultNamespace).Value = GetNotEmptyValueFromList(config.PublisherDisplayName, config.PublisherName, "Publisher");
            GetOrCreateNode(properties, "Logo", defaultNamespace).Value = "Assets\\Logo.png";

            var applicationsNode = GetOrCreateNode(package, "Applications", defaultNamespace);

            var usedNames = new HashSet<string>();
            foreach (var item in entryPoints)
            {
                var applicationNode = CreateApplicationNodeFromExe(baseDirectory, item);
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

            var branding = new MsixHeroBrandingInjector();
            await branding.Inject(template, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferIncoming).ConfigureAwait(false);
        }

        private static XElement CreateApplicationNodeFromExe(DirectoryInfo directoryInfo, string relativePath)
        {
            var fullFilePath = Path.Combine(directoryInfo.FullName, relativePath);
            if (!File.Exists(fullFilePath))
            {
                throw new FileNotFoundException($"File '{relativePath}' was not found in base directory {directoryInfo.FullName}.");
            }

            XNamespace defaultNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace nsUap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
            var applicationNode = new XElement(defaultNamespace + "Application");
            
            var visualElements = new XElement(nsUap + "VisualElements");

            var fileInfo = FileVersionInfo.GetVersionInfo(fullFilePath);
            var displayName = GetNotEmptyValueFromList(3, fileInfo.ProductName, fileInfo.InternalName, Path.GetFileNameWithoutExtension(fullFilePath), "Package display name");
            var description = GetNotEmptyValueFromList(3, fileInfo.FileDescription, fileInfo.ProductName, Path.GetFileNameWithoutExtension(fullFilePath), "Package description");
            var appId = GetNotEmptyValueFromList(3, Regex.Replace(Path.GetFileNameWithoutExtension(relativePath), "[^a-zA-z0-9_]+", string.Empty), "App1");
            
            applicationNode.SetAttributeValue("Id", appId);
            applicationNode.SetAttributeValue("EntryPoint", "Windows.FullTrustApplication");
            applicationNode.SetAttributeValue("Executable", relativePath);

            visualElements.SetAttributeValue("DisplayName", displayName);
            visualElements.SetAttributeValue("Square150x150Logo", "Assets/Logo.png");
            visualElements.SetAttributeValue("Square44x44Logo", "Assets/Logo.png");
            visualElements.SetAttributeValue("BackgroundColor", "#333333");
            visualElements.SetAttributeValue("Description", description);

            applicationNode.Add(visualElements);

            return applicationNode;
        }

        private static string GetNotEmptyValueFromList(int minimumLength, params string[] values)
        {
            if (minimumLength <= 0)
            {
                return GetNotEmptyValueFromList(values);
            }

            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v) && v.Length >= minimumLength);
        }

        private static string GetNotEmptyValueFromList(params string[] values)
        {
            return values.FirstOrDefault(v => !string.IsNullOrEmpty(v));
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
                throw new FileNotFoundException(string.Format(Resources.Localization.Packages_Error_MissingResource_Format, path), path);
            }

            return path;
        }

        private async IAsyncEnumerable<CreatedItem> CreateRegistryEntries(FileInfo regFile)
        {
            var tempDirectory = Path.GetTempPath();

            var p1 = Path.Combine(tempDirectory, "Registry" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".dat");
            var p2 = Path.Combine(tempDirectory, "User" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".dat");
            var p3 = Path.Combine(tempDirectory, "User.Classes" + Guid.NewGuid().ToString("N").Substring(0, 10) + ".dat");

            var writer = new MsixRegistryFileWriter(p1, p2, p3);

            writer.ImportRegFile(regFile.FullName);
            if (await writer.Flush().ConfigureAwait(false))
            {
                if (File.Exists(p1))
                {
                    yield return new CreatedItem(p1, "Registry.dat", CreatedItem.ItemType.Registry);
                }

                if (File.Exists(p2))
                {
                    yield return new CreatedItem(p2, "User.dat", CreatedItem.ItemType.Registry);
                }

                if (File.Exists(p3))
                {
                    yield return new CreatedItem(p3, "User.Classes.dat", CreatedItem.ItemType.Registry);
                }
            }
        }
    }
}
