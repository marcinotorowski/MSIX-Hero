// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files.Helpers;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Facades;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Packaging.Packer;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.ModificationPackages
{
    public class ModificationPackageBuilder : IModificationPackageBuilder
    {
        private static readonly LogSource Logger = new();
        private readonly IAppxPacker packer;

        public ModificationPackageBuilder(IAppxPacker packer)
        {
            this.packer = packer;
        }

        public async Task Create(ModificationPackageConfig config, string filePath, ModificationPackageBuilderAction action, CancellationToken cancellation = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Directory?.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            var modPackageTemplate = GetBundledResourcePath("ModificationPackage.AppxManifest.xml");
            var logoPath = GetBundledResourcePath("Logo.png");

            string manifestContent;
            await using (var fs = File.OpenRead(modPackageTemplate))
            {
                var xmlDoc = await XDocument.LoadAsync(fs, LoadOptions.None, cancellationToken: cancellation).ConfigureAwait(false);
                await this.PrepareModificationPackage(xmlDoc, config).ConfigureAwait(false);
                manifestContent = xmlDoc.ToString(SaveOptions.OmitDuplicateNamespaces);
            }

            if (action == ModificationPackageBuilderAction.Manifest)
            {
                await using (var sourceStream = File.OpenRead(logoPath))
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var assetsDirectory = Path.Combine(Path.GetDirectoryName(filePath), "Assets");
                    if (!Directory.Exists(assetsDirectory))
                    {
                        Directory.CreateDirectory(assetsDirectory);
                    }

                    await using var targetStream = File.OpenWrite(Path.Combine(assetsDirectory, "Logo.png"));
                    await sourceStream.CopyToAsync(targetStream, cancellation).ConfigureAwait(false);
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
                await using (var sourceStream = File.OpenRead(logoPath))
                {
                    var assetsFolder = Path.Combine(tempFolder, "Assets");
                    if (!Directory.Exists(assetsFolder))
                    {
                        Directory.CreateDirectory(assetsFolder);
                    }

                    await using (var targetStream = File.OpenWrite(Path.Combine(assetsFolder, "Logo.png")))
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

                var manifestPath = new FileInfo(Path.Join(tempFolder, FileConstants.AppxManifestFile));
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
                    Logger.Warn().WriteLine("Clean-up failed.");
                    Logger.Warn().WriteLine(e);
                }
            }
        }

        private async Task CopyRegistry(FileInfo regFile, DirectoryInfo targetDir)
        {
            var regWriter = new MsixRegistryFileWriter(targetDir.FullName);
            regWriter.ImportRegFile(regFile.FullName);
            await regWriter.Flush().ConfigureAwait(false);
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

                    await using var fileSrc = File.OpenRead(file.FullName);
                    await using var fileTarget = File.OpenWrite(targetFile.FullName);
                    await fileSrc.CopyToAsync(fileTarget, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private void CopyVfsStructure(string inputPackage, FileSystemInfo to)
        {
            var listOfFoldersToCreate = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.Equals(FileConstants.AppxManifestFile, Path.GetFileName(inputPackage)))
            {
                var baseDir = Path.GetDirectoryName(inputPackage);
                // ReSharper disable once AssignNullToNotNullAttribute
                var folders = Directory.EnumerateDirectories(baseDir, "*", SearchOption.AllDirectories);
                foreach (var folder in folders)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    listOfFoldersToCreate.Add(Path.GetRelativePath(baseDir, folder));
                }
            }
            else
            {
                switch (Path.GetExtension(inputPackage).ToLowerInvariant())
                {
                    case FileConstants.MsixExtension:
                    case FileConstants.AppxExtension:
                    {
                        using var sourceStream = File.OpenRead(inputPackage);
                        using var zip = new ZipArchive(sourceStream);
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

                        break;
                    }
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

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        private async Task PrepareModificationPackage(XDocument template, ModificationPackageConfig config)
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

            var parentName = config.ParentName;
            var parentPublisher = config.ParentPublisher;

            if (string.IsNullOrEmpty(parentPublisher) || string.IsNullOrEmpty(parentName))
            {
                IAppxFileReader reader = null;
                try
                {
                    if (string.Equals(FileConstants.AppxManifestFile, Path.GetFileName(config.ParentPackagePath), StringComparison.OrdinalIgnoreCase))
                    {
                        reader = new FileInfoFileReaderAdapter(config.ParentPackagePath);
                    }
                    else
                    {
                        reader = new ZipArchiveFileReaderAdapter(config.ParentPackagePath);
                    }

                    var manifestReader = new AppxManifestReader();
                    var read = await manifestReader.Read(reader).ConfigureAwait(false);

                    if (string.IsNullOrEmpty(parentPublisher))
                    {
                        parentPublisher = read.Publisher;
                    }

                    if (string.IsNullOrEmpty(parentName))
                    {
                        parentName = read.Name;
                    }
                }
                finally
                {
                    reader?.Dispose();
                }
            }

            dependency.SetAttributeValue("Name", parentName);
            dependency.SetAttributeValue("Publisher", parentPublisher);

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

            // Set identity
            var setIdentity = new SetPackageIdentity
            {
                Name = config.Name,
                Publisher = config.Publisher,
                Version = new Version(major, minor, build, revision).ToString()
            };
            await new SetPackageIdentityExecutor(template).Execute(setIdentity).ConfigureAwait(false);

            // Set properties
            var setProperties = new SetPackageProperties
            {
                DisplayName = config.DisplayName ?? "Modification Package Name",
                PublisherDisplayName = config.DisplayPublisher ?? "Modification Package Publisher Name",
                Description = "Modification Package for " + parentName,
                Logo = "Assets\\Logo.png",
                ModificationPackage = true
            };
            await new SetPackagePropertiesExecutor(template).Execute(setProperties).ConfigureAwait(false);

            // Set build metadata
            var branding = new MsixHeroBrandingInjector();
            await branding.Inject(template).ConfigureAwait(false);
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