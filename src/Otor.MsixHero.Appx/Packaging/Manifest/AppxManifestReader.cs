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
using System.Xml;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Diagnostic.System;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.Manifest.Helpers;
using Otor.MsixHero.Appx.Packaging.Manifest.Parsers;
using Otor.MsixHero.Appx.Psf;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Packaging.Manifest
{
    public class AppxManifestReader : IAppxManifestReader
    {
        private static readonly XNamespace NamespaceWindows10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
        private static readonly XNamespace NamespaceAppx = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");
        private static readonly XNamespace NamespaceUap10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/10");
        private static readonly XNamespace NamespaceBuild = XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");
        
        protected readonly AppxManifestApplicationParser ApplicationParser = new AppxManifestApplicationParser();
        protected readonly AppxExtensionsParser ExtensionsParser = new AppxExtensionsParser();
        protected readonly PsfReader PsfReader = new PsfReader();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppxManifestReader));

        public Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            var isMsix = fileReader.FileExists(FileConstants.AppxManifestFile);
            if (isMsix)
            {
                return this.ReadMsix(fileReader, FileConstants.AppxManifestFile, cancellationToken);
            }

            var isAppxBundle = fileReader.FileExists(FileConstants.AppxBundleManifestFilePath);
            if (isAppxBundle)
            {
                throw new NotSupportedException("Bundles are not supported.");
            }

            throw new NotSupportedException("Required package source is not supported.");
        }
        
        public async Task<AppxPackage> Read(IAppxFileReader fileReader, bool resolveDependencies, CancellationToken cancellationToken = default)
        {
            var result = await this.Read(fileReader, cancellationToken).ConfigureAwait(false);
            if (resolveDependencies)
            {
                foreach (var item in result.PackageDependencies)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using IAppxFileReader tempReader = new PackageIdentityFileReaderAdapter(PackageContext.CurrentUser, item.Name, item.Publisher, item.Version);
                    try
                    {
                        item.Dependency = await this.Read(tempReader, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        Logger.Warn("Could not read a dependency to {0} {2} by {1}", item.Name, item.Publisher, item.Version);
                    }
                }
            }
            
            return result;
        }

        public async Task<AppxBundle> ReadBundle(IAppxFileReader fileReader, CancellationToken cancellationToken)
        {
            var bundle = new AppxBundle();
            using (var file = fileReader.GetFile(FileConstants.AppxBundleManifestFilePath))
            {
                var document = await XDocument.LoadAsync(file, LoadOptions.None, cancellationToken).ConfigureAwait(false);
                if (document.Root == null)
                {
                    throw new FormatException("The manifest is malformed. There is no root element.");
                }
                
                var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2013/bundle");
                
                var identity = document.Root.Element(ns + "Identity");
                if (identity == null)
                {
                    throw new FormatException("The manifest is malformed, missing <Identity />.");
                }

                bundle.Version = identity.Attribute("Version")?.Value;
                bundle.Name = identity.Attribute("Name")?.Value;
                bundle.Publisher = identity.Attribute("Publisher")?.Value;
            }

            return bundle;
        }

        private async Task<AppxPackage> ReadMsix(IAppxFileReader fileReader, string manifestFileName, CancellationToken cancellationToken = default)
        {
            await using var xmlManifestStream = fileReader.GetFile(manifestFileName);
            var document = await XDocument.LoadAsync(xmlManifestStream, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            if (document.Root == null)
            {
                throw new FormatException("The manifest is malformed. There is no root element.");
            }

            if (document.Root == null)
            {
                throw new FormatException("The manifest is malformed. The document root must be <Package /> element.");
            }

            if (document.Root.Name.LocalName != "Package")
            {
                throw new FormatException("The manifest is malformed. The document root must be <Package /> element.");
            }

            if (document.Root.Name.Namespace != NamespaceWindows10 && document.Root.Name.Namespace != NamespaceAppx)
            {
                throw new FormatException("The manifest is malformed. The document root must be <Package /> element, belonging to a supported namespace.");
            }

            var nodePackage = document.Root;

            var nodeIdentity = nodePackage.Element(NamespaceWindows10 + "Identity") ?? nodePackage.Element(NamespaceAppx + "Identity");
            var nodeCapabilitiesRoot = nodePackage.Element(NamespaceWindows10 + "Capabilities") ?? nodePackage.Element(NamespaceAppx + "Capabilities");
            var nodeDependenciesRoot = nodePackage.Element(NamespaceWindows10 + "Dependencies") ?? nodePackage.Element(NamespaceAppx + "Dependencies");
            var nodePrerequisitesRoot = nodePackage.Element(NamespaceWindows10 + "Prerequisites") ?? nodePackage.Element(NamespaceAppx + "Prerequisites");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeBuild = nodePackage.Element(NamespaceBuild + "Metadata");

            var appxPackage = new AppxPackage();

            if (fileReader is IAppxDiskFileReader diskReader)
            {
                appxPackage.RootFolder = diskReader.RootDirectory;
            }
            else if (fileReader is ZipArchiveFileReaderAdapter zipArchiveReader)
            {
                appxPackage.RootFolder = Path.GetDirectoryName(zipArchiveReader.PackagePath);
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            if (nodeIdentity != null)
            {
                appxPackage.Name = nodeIdentity.Attribute("Name")?.Value;
                var procArch = nodeIdentity.Attribute("ProcessorArchitecture")?.Value;
                if (Enum.TryParse(typeof(AppxPackageArchitecture), procArch ?? string.Empty, true, out object parsedArchitecture) && parsedArchitecture != null)
                {
                    appxPackage.ProcessorArchitecture = (AppxPackageArchitecture)parsedArchitecture;
                }
                else
                {
                    appxPackage.ProcessorArchitecture = AppxPackageArchitecture.Neutral;
                }

                appxPackage.Publisher = nodeIdentity.Attribute("Publisher")?.Value;
                appxPackage.Version = nodeIdentity.Attribute("Version")?.Value;
            }
            
            var nodeProperties = nodePackage.Element(NamespaceWindows10 + "Properties") ?? nodePackage.Element(NamespaceAppx + "Properties");
            await this.SetProperties(fileReader, appxPackage, nodeProperties, cancellationToken).ConfigureAwait(false);
            
            appxPackage.PackageDependencies = new List<AppxPackageDependency>();
            appxPackage.HostPackageDependencies = new List<AppxHostRuntimeDependency>();
            appxPackage.OperatingSystemDependencies = new List<AppxOperatingSystemDependency>();
            appxPackage.Applications = new List<AppxApplication>();
            appxPackage.Extensions = new List<AppxExtension>();

            var dependencies = nodeDependenciesRoot?.Elements();

            foreach (var node in dependencies ?? Enumerable.Empty<XElement>())
            {
                switch (node.Name.LocalName)
                {
                    case "MainPackageDependency":
                        var modName = node.Attribute("Name")?.Value;

                        var appxModPackDependency = new AppxMainPackageDependency
                        {
                            Name = modName,
                        };

                        appxPackage.MainPackage = appxModPackDependency;

                        break;
                    case "HostRuntimeDependency":
                        if (node.Name.Namespace == NamespaceUap10)
                        {
                            var runtimeDependency = new AppxHostRuntimeDependency
                            {
                                MinVersion = node.Attribute("MinVersion")?.Value,
                                Name = node.Attribute("Name")?.Value,
                                Publisher = node.Attribute("Publisher")?.Value
                            };
                            
                            appxPackage.HostPackageDependencies.Add(runtimeDependency);
                        }
                        
                        break;
                        
                    case "TargetDeviceFamily":
                    {
                        /*
                            <TargetDeviceFamily MaxVersionTested="10.0.15063.0" MinVersion="10.0.15063.0" Name="Windows.Universal" />
                            */

                        var minVersion = node.Attribute("MinVersion")?.Value;
                        var maxVersion = node.Attribute("MaxVersionTested")?.Value;
                        var name = node.Attribute("Name")?.Value;

                        appxPackage.OperatingSystemDependencies.Add(new AppxOperatingSystemDependency
                        {
                            Minimum = Windows10Parser.GetOperatingSystemFromNameAndVersion(name, minVersion),
                            Tested = Windows10Parser.GetOperatingSystemFromNameAndVersion(name, maxVersion),
                        });

                        break;
                    }

                    case "PackageDependency":
                    {
                        /*
                            <PackageDependency MinVersion="1.4.24201.0" Name="Microsoft.NET.Native.Runtime.1.4" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US" />
                            */

                        var minVersion = node.Attribute("MinVersion")?.Value;
                        var name = node.Attribute("Name")?.Value;
                        var publisher = node.Attribute("Publisher")?.Value;

                        var appxDependency = new AppxPackageDependency
                        {
                            Publisher = publisher,
                            Name = name,
                            Version = minVersion
                        };

                        appxPackage.PackageDependencies.Add(appxDependency);
                        break;
                    }
                }
            }

            if (nodePrerequisitesRoot != null)
            {
                var min = nodePrerequisitesRoot.Element(NamespaceAppx + "OSMinVersion")?.Value;
                var max = nodePrerequisitesRoot.Element(NamespaceAppx + "OSMaxVersionTested")?.Value;
                    
                appxPackage.OperatingSystemDependencies.Add(new AppxOperatingSystemDependency
                {
                    Minimum = min == null ? null : Windows10Parser.GetOperatingSystemFromNameAndVersion("Windows.Desktop", min),
                    Tested = max == null ? null : Windows10Parser.GetOperatingSystemFromNameAndVersion("Windows.Desktop", max),
                });
            }

            if (nodeBuild != null)
            {
                var buildKeyValues = new Dictionary<string, string>();

                foreach (var buildNode in nodeBuild.Elements(NamespaceBuild + "Item"))
                {
                    var attrName = buildNode.Attribute("Name")?.Value;
                    if (attrName == null)
                    {
                        continue;
                    }

                    var attrVersion = buildNode.Attribute("Version")?.Value;
                    if (attrVersion == null)
                    {
                        attrVersion = buildNode.Attribute("Value")?.Value;
                        if (attrVersion == null)
                        {
                            continue;
                        }
                    }

                    buildKeyValues[attrName] = attrVersion;
                }

                var appDetector = new AuthoringAppDetector(fileReader);
                if (appDetector.TryDetectAny(buildKeyValues, out var buildInfo))
                {
                    appxPackage.BuildInfo = buildInfo;
                }
            }

            appxPackage.FamilyName = AppxPackaging.GetPackageFamilyName(appxPackage.Name, appxPackage.Publisher);
            appxPackage.FullName = AppxPackaging.GetPackageFullName(appxPackage.Name, appxPackage.Publisher, appxPackage.ProcessorArchitecture, appxPackage.Version, appxPackage.ResourceId);
            appxPackage.Capabilities = this.GetCapabilities(nodeCapabilitiesRoot);
            
            await foreach (var app in this.ApplicationParser.ParseManifest(fileReader, document, cancellationToken).ConfigureAwait(false))
            {
                appxPackage.Applications.Add(app);
                app.Logo ??= appxPackage.Logo;
            }

            var extensionsNode = document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "Extensions");
            foreach (var ext in this.ExtensionsParser.ParseManifest(extensionsNode))
            {
                appxPackage.Extensions.Add(ext);
            }

            foreach (var psfApp in appxPackage.Applications.Where(a => PackageTypeConverter.GetPackageTypeFrom(appxPackage, a) == MsixPackageType.BridgePsf))
            {
                cancellationToken.ThrowIfCancellationRequested();
                psfApp.Psf = this.PsfReader.Read(psfApp.Id, psfApp.Executable, fileReader);
            }

            var pkgManager = new PackageManager();
            var pkg = pkgManager.FindPackageForUser(string.Empty, appxPackage.FullName);
            if (pkg == null && appxPackage.ResourceId == null)
            {
                appxPackage.FullName = AppxPackaging.GetPackageFullName(appxPackage.Name, appxPackage.Publisher, appxPackage.ProcessorArchitecture, appxPackage.Version, "neutral");
                pkg = pkgManager.FindPackageForUser(string.Empty, appxPackage.FullName);
            }

            string manifestFilePath;
            if (pkg?.InstalledLocation != null)
            {
                manifestFilePath = Path.Combine(pkg.InstalledLocation.Path, FileConstants.AppxManifestFile);
            }
            else if (fileReader is IAppxDiskFileReader appxDiskReader)
            {
                manifestFilePath = Path.Combine(appxDiskReader.RootDirectory, manifestFileName);
            }
            else
            {
                manifestFilePath = manifestFileName;
            }

            if (pkg == null)
            {
                appxPackage.Source = new NotInstalledSource();
            }
            else if (pkg.SignatureKind == PackageSignatureKind.System)
            {
                appxPackage.Source = new SystemSource(manifestFilePath);
            }
            else if (pkg.SignatureKind == PackageSignatureKind.None || pkg.IsDevelopmentMode)
            {
                appxPackage.Source = new DeveloperSource(Path.Combine(appxPackage.RootFolder, manifestFileName));
            }
            else if (pkg.SignatureKind == PackageSignatureKind.Store)
            {
                appxPackage.Source = new StorePackageSource(appxPackage.FamilyName, Path.GetDirectoryName(manifestFilePath));
            }
            else
            {
                var appInstaller = pkg.GetAppInstallerInfo();
                if (appInstaller != null)
                {
                    appxPackage.Source = new AppInstallerPackageSource(appInstaller.Uri, Path.GetDirectoryName(manifestFilePath));
                }
            }

            appxPackage.Source ??= new StandardSource(manifestFilePath);

            return await Translate(fileReader, appxPackage, cancellationToken).ConfigureAwait(false);
        }

        private async Task SetProperties(IAppxFileReader fileReader, AppxPackage appxPackage, XElement propertiesNode, CancellationToken cancellationToken = default)
        {
            if (propertiesNode != null)
            {
                foreach (var node in propertiesNode.Elements())
                {
                    switch (node.Name.LocalName)
                    {
                        case "Logo":
                            var logo = node.Value;
                            if (!string.IsNullOrEmpty(logo))
                            {
                                await using var resourceStream = fileReader.GetResource(logo);
                                if (resourceStream != null)
                                {
                                    await using var memoryStream = new MemoryStream();
                                    await resourceStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                                    await memoryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    appxPackage.Logo = memoryStream.ToArray();
                                }
                            }

                            break;

                        case "DisplayName":
                            appxPackage.DisplayName = node.Value;
                            break;

                        case "PublisherDisplayName":
                            appxPackage.PublisherDisplayName = node.Value;
                            break;

                        case "PackageIntegrity":
                            var packageIntegrityContent = propertiesNode.Element(NamespaceUap10 + "Content");
                            if (packageIntegrityContent != null)
                            {
                                appxPackage.PackageIntegrity = packageIntegrityContent.Attribute("Enforcement")?.Value == "on";
                            }

                            break;

                        case "Framework":
                            appxPackage.IsFramework = string.Equals(node.Value, "true", StringComparison.OrdinalIgnoreCase);
                            break;

                        case "Description":
                            appxPackage.Description = node.Value;
                            break;
                    }
                }
            }
        }

        private static async Task<AppxPackage> Translate(IAppxFileReader fileReader, AppxPackage package, CancellationToken cancellationToken)
        {
            var translationRequired = package.DisplayName?.StartsWith("ms-resource:") == true
              || package.PublisherDisplayName?.StartsWith("ms-resource:") == true
              || package.Description?.StartsWith("ms-resource:") == true
              || package.Applications.Any(a => a.DisplayName?.StartsWith("ms-resource:") == true)
              || package.Applications.Any(a => a.Description?.StartsWith("ms-resource:") == true);

            if (!translationRequired)
            {
                return package;
            }

            string priFullPath = null;
            var cleanup = false;

            try
            {
                if (fileReader.FileExists("resources.pri"))
                {
                    using (var stream = fileReader.GetFile("resources.pri"))
                    {
                        if (stream is FileStream fileStream)
                        {
                            priFullPath = fileStream.Name;
                        }
                        else
                        {
                            priFullPath = Path.GetTempFileName();
                            cleanup = true;

                            using (var fs = File.OpenWrite(priFullPath))
                            {
                                await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                                await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }

                    package.DisplayName = StringLocalizer.Localize(priFullPath, package.Name, package.FullName, package.DisplayName);
                    package.Description = StringLocalizer.Localize(priFullPath, package.Name, package.FullName, package.Description);
                    package.PublisherDisplayName = StringLocalizer.Localize(priFullPath, package.Name, package.FullName, package.PublisherDisplayName);

                    if (string.IsNullOrEmpty(package.DisplayName))
                    {
                        package.DisplayName = package.Name;
                    }
                    
                    if (string.IsNullOrEmpty(package.PublisherDisplayName))
                    {
                        package.PublisherDisplayName = package.Publisher;
                    }
                    
                    foreach (var app in package.Applications ?? Enumerable.Empty<AppxApplication>())
                    {
                        app.DisplayName = StringLocalizer.Localize(priFullPath, app.Id, package.FullName, app.DisplayName);
                        
                        if (string.IsNullOrEmpty(app.DisplayName))
                        {
                            app.DisplayName = app.Id;
                        }    
                        
                        app.Description = StringLocalizer.Localize(priFullPath, app.Id, package.FullName, app.Description);
                    }
                }

                return package;
            }
            finally
            {
                if (cleanup && File.Exists(priFullPath))
                {
                    File.Delete(priFullPath);
                }
            }
        }

        private List<AppxCapability> GetCapabilities(XElement nodeCapabilitiesRoot)
        {
            if (nodeCapabilitiesRoot == null || !nodeCapabilitiesRoot.HasElements)
            {
                return new List<AppxCapability>();
            }

            var list = new List<AppxCapability>();

            var restricted = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities");

            foreach (var node in nodeCapabilitiesRoot.Elements())
            {
                if (node.NodeType == XmlNodeType.Comment)
                {
                    continue;
                }

                var type = CapabilityType.Custom;
                var name = node.Attribute("Name")?.Value;

                if (node.Name.Namespace == restricted)
                {
                    type = CapabilityType.Restricted;
                }
                else
                {
                    switch (node.Name.LocalName)
                    {
                        case "DeviceCapability":
                            type = CapabilityType.Device;
                            break;
                        case "Capability":
                            type = CapabilityType.General;
                            break;
                    }
                }

                list.Add(new AppxCapability { Name = name, Type = type });
            }

            return list;
        }
    }
}