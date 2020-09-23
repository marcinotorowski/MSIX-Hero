using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Infrastructure.Branding;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Registry.Converter;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Appx.Packaging.ModificationPackages
{
    public class AppxContentBuilder : IAppxContentBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IAppxPacker packer;

        public AppxContentBuilder(IAppxPacker packer)
        {
            this.packer = packer;
        }

        public async Task Create(ModificationPackageConfig config, string filePath, ModificationPackageBuilderAction action, CancellationToken cancellation = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var modPackageTemplate = GetBundledResourcePath("ModificationPackage.AppxManifest.xml");
            var logoPath = GetBundledResourcePath("Logo.png");

            string manifestContent;
            using (var fs = File.OpenRead(modPackageTemplate))
            {
                var xmlDoc = await XDocument.LoadAsync(fs, LoadOptions.None, cancellationToken: cancellation).ConfigureAwait(false);
                this.PrepareModificationPackage(xmlDoc, config);
                manifestContent = xmlDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
            }

            if (action == ModificationPackageBuilderAction.Manifest)
            {
                using (var sourceStream = File.OpenRead(logoPath))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var assetsDirectory = Path.Combine(Path.GetDirectoryName(filePath), "Assets");
                    if (!Directory.Exists(assetsDirectory))
                    {
                        Directory.CreateDirectory(assetsDirectory);
                    }
                    
                    using (var targetStream = File.OpenWrite(Path.Combine(assetsDirectory, "Logo.png")))
                    {
                        await sourceStream.CopyToAsync(targetStream, cancellation).ConfigureAwait(false);
                    }
                }

                if (config.IncludeVfsFolders && config.ParentPackagePath != null)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    this.CopyVfsStructure(config.ParentPackagePath, new DirectoryInfo(Path.GetDirectoryName(filePath)));
                }

                if (config.IncludeFolder != null)
                {
                    if (!config.IncludeFolder.Exists)
                    {
                        throw new DirectoryNotFoundException($"The directory folder '{config.IncludeFolder.FullName}' does not exist.");
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    await this.CopyFolder(config.IncludeFolder, new DirectoryInfo(Path.GetDirectoryName(filePath)), cancellation).ConfigureAwait(false);
                }

                if (config.IncludeRegistry?.Exists == true)
                {
                    if (!config.IncludeRegistry.Exists)
                    {
                        throw new FileNotFoundException(
                            $"The file '{config.IncludeRegistry.FullName}' does not exist.");
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    await this.CopyRegistry(config.IncludeRegistry, new DirectoryInfo(Path.GetDirectoryName(filePath))).ConfigureAwait(false);
                }

                await File.WriteAllTextAsync(filePath, manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);

                return;
            }

            var tempFolder = Environment.ExpandEnvironmentVariables(@"%temp%\msix-hero-" + Guid.NewGuid().ToString("N").Substring(10));
            try
            {   
                using (var sourceStream = File.OpenRead(logoPath))
                {
                    var assetsFolder = Path.Combine(tempFolder, "Assets");
                    if (!Directory.Exists(assetsFolder))
                    {
                        Directory.CreateDirectory(assetsFolder);
                    }

                    using (var targetStream = File.OpenWrite(Path.Combine(assetsFolder, "Logo.png")))
                    {
                        await sourceStream.CopyToAsync(targetStream, cancellation).ConfigureAwait(false);
                    }
                }

                if (config.IncludeVfsFolders && config.ParentPackagePath != null)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    this.CopyVfsStructure(config.ParentPackagePath, new DirectoryInfo(tempFolder));
                }

                if (config.IncludeFolder != null)
                {
                    if (!config.IncludeFolder.Exists)
                    {
                        throw new DirectoryNotFoundException($"The directory folder '{config.IncludeFolder.FullName}' does not exist.");
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    await this.CopyFolder(config.IncludeFolder, new DirectoryInfo(tempFolder), cancellation).ConfigureAwait(false);
                }

                if (config.IncludeRegistry != null)
                {
                    if (!config.IncludeRegistry.Exists)
                    {
                        throw new FileNotFoundException($"The file '{config.IncludeRegistry.FullName}' does not exist.");
                    }

                    await this.CopyRegistry(config.IncludeRegistry, new DirectoryInfo(tempFolder)).ConfigureAwait(false);
                }

                var manifestPath = new FileInfo(Path.Join(tempFolder, "AppxManifest.xml"));
                if (manifestPath.Exists)
                {
                    manifestPath.Delete();
                }
                else if (manifestPath.Directory?.Exists == false)
                {
                    manifestPath.Directory.Create();
                }

                await File.WriteAllTextAsync(manifestPath.FullName, manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);
                await this.packer.PackFiles(tempFolder, filePath, 0, cancellation, progress).ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempFolder))
                    {
                        Directory.Delete(tempFolder, true);
                    }
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Clean-up failed.");
                }
            }
        }

        private async Task CopyRegistry(FileInfo regFile, DirectoryInfo targetDir)
        {
            var writer = new RegConverter();

            var pathAll = Path.Combine(targetDir.FullName, "Registry.dat");
            var pathUser = Path.Combine(targetDir.FullName, "User.dat");
            var pathHkcr = Path.Combine(targetDir.FullName, "UserClasses.dat");
            await writer.ConvertFromRegToDat(regFile.FullName, pathAll).ConfigureAwait(false);
            await writer.ConvertFromRegToDat(regFile.FullName, pathUser, RegistryRoot.HKEY_CURRENT_USER).ConfigureAwait(false);
            await writer.ConvertFromRegToDat(regFile.FullName, pathHkcr, RegistryRoot.HKEY_CLASSES_ROOT).ConfigureAwait(false);
        }

        private async Task CopyFolder(DirectoryInfo from, DirectoryInfo to, CancellationToken cancellationToken)
        {
            var folders = new Queue<DirectoryInfo>();
            folders.Enqueue(from);

            while (folders.TryDequeue(out var entry))
            {
                foreach (var subfolder in entry.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    folders.Enqueue(subfolder);
                }

                var targetPath = Path.Combine(to.FullName, Path.GetRelativePath(from.FullName, entry.FullName));
                var targetEntry = new DirectoryInfo(targetPath);
                if (!targetEntry.Exists)
                {
                    targetEntry.Create();
                }

                foreach (var file in entry.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                {
                    var targetFile = new FileInfo(Path.Combine(targetEntry.FullName, file.Name));

                    using (var fileSrc = File.OpenRead(file.FullName))
                    {
                        using (var fileTarget = File.OpenWrite(targetFile.FullName))
                        {
                            await fileSrc.CopyToAsync(fileTarget, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private void CopyVfsStructure(string inputPackage, FileSystemInfo to)
        {
            var listOfFoldersToCreate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            switch (Path.GetExtension(inputPackage).ToLowerInvariant())
            {
                case ".msix":
                case ".appx":
                {
                    using (var sourceStream = File.OpenRead(inputPackage))
                    {
                        using (var zip = new ZipArchive(sourceStream))
                        {
                            var entries = zip.Entries.Where(e => e.FullName.StartsWith("VFS/"));
                            foreach (var entry in entries)
                            {
                                if (entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                                {
                                    listOfFoldersToCreate.Add(Uri.UnescapeDataString(entry.FullName.TrimEnd('/')));
                                }
                                else
                                {
                                    var dir = Path.GetDirectoryName(entry.FullName);
                                    if (!string.IsNullOrEmpty(dir))
                                    {
                                        listOfFoldersToCreate.Add(Uri.UnescapeDataString(dir));
                                    }
                                }
                            }
                        }
                    }

                    break;
                }
                case ".appxmanifest":
                {
                    var baseDir = Path.GetDirectoryName(inputPackage);
                    var folders = Directory.EnumerateDirectories(baseDir, "*", SearchOption.AllDirectories);
                    foreach (var folder in folders)
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        listOfFoldersToCreate.Add(Path.GetRelativePath(baseDir, folder));
                    }

                    break;
                }
            }

            foreach (var folder in listOfFoldersToCreate)
            {
                var dirInfo = new DirectoryInfo(Path.Combine(to.FullName, folder));
                if (dirInfo.Exists)
                {
                    continue;
                }

                dirInfo.Create();
            }
        }

        private void PrepareModificationPackage(XDocument template, ModificationPackageConfig config)
        {
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
            var dependencies = GetOrCreateNode(package, "Dependencies", defaultNamespace);

            var dependency = new XElement(nsUap4 + "MainPackageDependency");
            dependencies.Add(dependency);
            dependency.SetAttributeValue("Name", config.ParentName);
            dependency.SetAttributeValue("Publisher", config.ParentPublisher);

            var identity = GetOrCreateNode(package, "Identity", defaultNamespace);
            identity.SetAttributeValue("Name", config.Name);
            identity.SetAttributeValue("Publisher", config.Publisher);

            var fixVersion = Version.Parse(config.Version);

            var major = fixVersion.Major;
            var minor = fixVersion.Minor;
            var build = fixVersion.Build;
            var revision = fixVersion.Revision;
            
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
            GetOrCreateNode(properties, "DisplayName", defaultNamespace).Value = config.DisplayName ?? "Modification Package Name";
            GetOrCreateNode(properties, "PublisherDisplayName", defaultNamespace).Value = config.DisplayPublisher ?? "Modification Package Publisher Name";
            GetOrCreateNode(properties, "Description", defaultNamespace).Value = "Modification Package for " + config.ParentName;
            GetOrCreateNode(properties, "Logo", defaultNamespace).Value = "Assets\\Logo.png";
            GetOrCreateNode(properties, "ModificationPackage", nsRescap6).Value = "true";

            var branding = new MsixHeroBrandingInjector();
            branding.Inject(template);
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
    }
}