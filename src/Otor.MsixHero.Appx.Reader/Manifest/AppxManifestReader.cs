﻿// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Dapplo.Log;
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Common.Identity;
using Otor.MsixHero.Appx.Common.WindowsVersioning;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.File.Adapters;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Appx.Reader.Manifest.Helpers;
using Otor.MsixHero.Appx.Reader.Psf;

namespace Otor.MsixHero.Appx.Reader.Manifest
{
    public class AppxManifestReader : IAppxManifestReader
    {
        public Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            var isMsix = fileReader.FileExists(AppxFileConstants.AppxManifestFile);
            if (isMsix)
            {
                return this.ReadMsix(fileReader, cancellationToken);
            }

            var isAppxBundle = fileReader.FileExists(AppxFileConstants.AppxBundleManifestFilePath);
            if (isAppxBundle)
            {
                return Task.FromException<AppxPackage>(new NotSupportedException(Resources.Localization.Packages_Error_BundleNotSupported));
            }

            return Task.FromException<AppxPackage>(new NotSupportedException(Resources.Localization.Packages_Error_SrcNotSupported));
        }

        public async Task<AppxBundle> ReadBundle(IAppxFileReader fileReader, CancellationToken cancellationToken)
        {
            var bundle = new AppxBundle();
            await using var file = fileReader.GetFile(AppxFileConstants.AppxBundleManifestFilePath);
            var document = await XDocument.LoadAsync(file, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            if (document.Root == null)
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Root);
            }

            // var b4 = XNamespace.Get("http://schemas.microsoft.com/appx/2018/bundle");
            // var b5 = XNamespace.Get("http://schemas.microsoft.com/appx/2019/bundle");
            var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2013/bundle");

            var identity = document.Root.Element(ns + "Identity");
            if (identity == null)
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Identity);
            }

            bundle.Version = identity.Attribute("Version")?.Value;
            bundle.Name = identity.Attribute("Name")?.Value;
            bundle.Publisher = identity.Attribute("Publisher")?.Value;

            return bundle;
        }

        private async Task<AppxPackage> ReadMsix(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            await using var file = fileReader.GetFile(AppxFileConstants.AppxManifestFile);
            var document = await XDocument.LoadAsync(file, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            if (document.Root == null)
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Root);
            }

            var ns =        XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
            var ns2 =       XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");
            var uap =       XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
            var uap10 =     XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/10");
            var uap5 =      XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/5");
            var uap3 =      XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/3");
            var desktop10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10");
            var desktop6 =  XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/6");
            var desktop2 =  XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/2");
            var build =     XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");

            if (document.Root == null)
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Root_Package);
            }

            if (document.Root.Name.LocalName != "Package")
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Root_Package);
            }

            if (document.Root.Name.Namespace != ns && document.Root.Name.Namespace != ns2)
            {
                throw new FormatException(Resources.Localization.Packages_Error_Manifest_Root_Package_Namespace);
            }

            var nodePackage = document.Root;

            cancellationToken.ThrowIfCancellationRequested();
            var nodeIdentity = nodePackage.Element(ns + "Identity") ?? nodePackage.Element(ns2 + "Identity");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeProperties = nodePackage.Element(ns + "Properties") ?? nodePackage.Element(ns2 + "Properties");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeApplicationsRoot = nodePackage.Element(ns + "Applications") ?? nodePackage.Element(ns2 + "Applications");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeCapabilitiesRoot = nodePackage.Element(ns + "Capabilities") ?? nodePackage.Element(ns2 + "Capabilities");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeDependenciesRoot = nodePackage.Element(ns + "Dependencies") ?? nodePackage.Element(ns2 + "Dependencies");

            cancellationToken.ThrowIfCancellationRequested();
            var nodePrerequisitesRoot = nodePackage.Element(ns + "Prerequisites") ?? nodePackage.Element(ns2 + "Prerequisites");

            cancellationToken.ThrowIfCancellationRequested();
            var nodeBuild = nodePackage.Element(build + "Metadata");

            var appxPackage = new AppxPackage();

            if (fileReader is IAppxDiskFileReader diskReader)
            {
                appxPackage.PackagePath = Path.Combine(diskReader.RootDirectory, AppxFileConstants.AppxManifestFile);
                appxPackage.RootFolder = diskReader.RootDirectory;
            }
            else if (fileReader is ZipArchiveFileReaderAdapter zipArchiveReader)
            {
                appxPackage.PackagePath = zipArchiveReader.PackagePath;
                appxPackage.RootFolder = Path.GetDirectoryName(zipArchiveReader.PackagePath);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (nodeIdentity != null)
            {
                appxPackage.Name = nodeIdentity.Attribute("Name")?.Value;
                var procArch = nodeIdentity.Attribute("ProcessorArchitecture")?.Value;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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

            cancellationToken.ThrowIfCancellationRequested();
            if (nodeProperties != null)
            {
                appxPackage.RegistryVirtualizationEnabled = nodeProperties.Element(desktop6 + "RegistryWriteVirtualization")?.Value != "disabled";
                appxPackage.FileVirtualizationEnabled = nodeProperties.Element(desktop6 + "FileSystemWriteVirtualization")?.Value != "disabled";
                foreach (var node in nodeProperties.Elements())
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
                            var packageIntegrityContent = nodeProperties.Element(uap10 + "Content");
                            if (packageIntegrityContent != null)
                            {
                                appxPackage.PackageIntegrity = packageIntegrityContent.Attribute("Enforcement")?.Value == "on";
                            }

                            break;

                        case "Framework":
                            appxPackage.IsFramework = string.Equals(node.Value ?? "false", "true", StringComparison.OrdinalIgnoreCase);
                            break;

                        case "Description":
                            appxPackage.Description = node.Value;
                            break;
                    }
                }
            }
            else
            {
                appxPackage.RegistryVirtualizationEnabled = true;
                appxPackage.FileVirtualizationEnabled = true;
            }

            cancellationToken.ThrowIfCancellationRequested();
            appxPackage.PackageDependencies = new List<AppxPackageDependency>();
            appxPackage.OperatingSystemDependencies = new List<AppxOperatingSystemDependency>();
            appxPackage.MainPackages = new List<AppxMainPackageDependency>();
            appxPackage.Applications = new List<AppxApplication>();

            if (nodeApplicationsRoot != null)
            {
                foreach (var node in nodeApplicationsRoot.Elements().Where(x => x.Name.LocalName == "Application" && (x.Name.Namespace == ns || x.Name.Namespace == ns2)))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    /*
                         <Application EntryPoint="SparklerApp.App" Executable="XD.exe" Id="App">
                            <uap:VisualElements BackgroundColor="#2D001E" Description="Adobe XD" DisplayName="Adobe XD" Square150x150Logo="Assets\xd_med_tile.png" Square44x44Logo="Assets\xd_app_list_icon.png">
                              <uap:DefaultTile Square310x310Logo="Assets\xd_large_tile.png" Square71x71Logo="Assets\xd_small_tile.png" Wide310x150Logo="Assets\xd_wide_tile.png">
                                <uap:ShowNameOnTiles>
                                  <uap:ShowOn Tile="square150x150Logo" />
                                  <uap:ShowOn Tile="wide310x150Logo" />
                                  <uap:ShowOn Tile="square310x310Logo" />
                                </uap:ShowNameOnTiles>
                              </uap:DefaultTile>
                              <uap:SplashScreen BackgroundColor="#FFFFFF" Image="Assets\xd_splash.png" />
                            </uap:VisualElements>

                         <Application Id="RayEval" Executable="PsfLauncher32.exe" EntryPoint="Windows.FullTrustApplication">
                            <uap:VisualElements DisplayName="RayEval" Description="RayEval Raynet GmbH" BackgroundColor="transparent" 
                                Square150x150Logo="Assets\Square150x150Logo.scale-100.png" Square44x44Logo="Assets\Square44x44Logo.scale-100.png">
                                <uap:DefaultTile Wide310x150Logo="Assets\Square310x150Logo.scale-100.png" Square310x310Logo="Assets\Square310x310Logo.scale-100.png" Square71x71Logo="Assets\Square71x71Logo.scale-100.png" />
                            </uap:VisualElements>
                            <Extensions />
                        </Application>  
                        */

                    var appxApplication = new AppxApplication
                    {
                        EntryPoint = node.Attribute("EntryPoint")?.Value,
                        StartPage = node.Attribute("StartPage")?.Value,
                        Executable = node.Attribute("Executable")?.Value,
                        Parameters = node.Attribute(uap10 + "Parameters")?.Value,
                        HostId = node.Attribute(uap10 + "HostId")?.Value,
                        Id = node.Attribute("Id")?.Value,
                        Extensions = new List<AppxExtension>()
                    };

                    /*
                        <Extensions><desktop6:Extension Category="windows.service" Executable="VFS\ProgramFilesX86\RayPackStudio\FloatingLicenseServer\FloatingLicenseServer.exe" EntryPoint="Windows.FullTrustApplication"><desktop6:Service Name="PkgSuiteFloatingLicenseServer" StartupType="auto" StartAccount="networkService" /></desktop6:Extension></Extensions>
                        */

                    var nodeExtensions = node.Elements().FirstOrDefault(e => e.Name.LocalName == "Extensions");

                    if (nodeExtensions != null)
                    {
                        foreach (var extension in nodeExtensions.Elements().Where(e => e.Name.LocalName == "Extension" && (e.Name.Namespace == ns || e.Name.Namespace == ns2 || e.Name.Namespace == desktop6 || e.Name.Namespace == desktop2 || e.Name.Namespace == uap5 || e.Name.Namespace == uap3)))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var category = extension.Attribute("Category")?.Value;

                            switch (category)
                            {
                                case "windows.appExecutionAlias":
                                    var aliasNode = extension.Element(uap3 + "AppExecutionAlias") ?? extension.Element(uap5 + "AppExecutionAlias");
                                    if (aliasNode != null)
                                    {
                                        var desktopExecAliases = aliasNode.Elements(desktop10 + "ExecutionAlias").Concat(aliasNode.Elements(uap5 + "ExecutionAlias"));
                                        foreach (var desktopExecAlias in desktopExecAliases)
                                        {
                                            if (appxApplication.ExecutionAlias == null)
                                            {
                                                appxApplication.ExecutionAlias = new List<string>();
                                            }

                                            appxApplication.ExecutionAlias.Add(desktopExecAlias.Attribute("Alias")?.Value);
                                        }
                                    }

                                    break;
                                case "windows.service":
                                    var serviceNode = extension.Element(desktop6 + "Service");
                                    if (serviceNode == null)
                                    {
                                        continue;
                                    }

                                    var service = new AppxService
                                    {
                                        Category = "windows.service"
                                    };

                                    service.EntryPoint = extension.Attribute("EntryPoint")?.Value;
                                    service.Executable = extension.Attribute("Executable")?.Value;

                                    service.Name = extension.Attribute("Name")?.Value;
                                    service.StartAccount = extension.Attribute("StartAccount")?.Value;
                                    service.StartupType = extension.Attribute("StartupType")?.Value;

                                    appxApplication.Extensions.Add(service);
                                    break;
                            }
                        }
                    }

                    var visualElements = node.Elements().FirstOrDefault(e => e.Name.LocalName == "VisualElements");
                    if (visualElements != null)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        appxApplication.Description = visualElements.Attribute("Description")?.Value;
                        appxApplication.DisplayName = visualElements.Attribute("DisplayName")?.Value;
                        appxApplication.BackgroundColor = visualElements.Attribute("BackgroundColor")?.Value;
                        appxApplication.Square150x150Logo = visualElements.Attribute("Square150x150Logo")?.Value;
                        appxApplication.Square44x44Logo = visualElements.Attribute("Square44x44Logo")?.Value;
                        appxApplication.Visible = visualElements.Attribute("AppListEntry")?.Value != "none";

                        var defaultTile = visualElements.Element(uap + "DefaultTile");
                        if (defaultTile != null)
                        {
                            appxApplication.Wide310x150Logo = defaultTile.Attribute("Wide310x150Logo")?.Value;
                            appxApplication.Square310x310Logo = defaultTile.Attribute("Square310x310Logo")?.Value;
                            appxApplication.Square71x71Logo = defaultTile.Attribute("Square71x71Logo")?.Value;
                            appxApplication.ShortName = defaultTile.Attribute("ShortName")?.Value;
                        }

                        var logo = appxApplication.Square44x44Logo ?? appxApplication.Square30x30Logo ?? appxApplication.Square71x71Logo ?? appxApplication.Square150x150Logo;
                        if (logo == null)
                        {
                            appxApplication.Logo = appxPackage.Logo;
                        }
                        else
                        {
                            await using var stream =
                                fileReader.GetResource(appxApplication.Square44x44Logo) ??
                                fileReader.GetResource(appxApplication.Square30x30Logo) ??
                                fileReader.GetResource(appxApplication.Square71x71Logo) ??
                                fileReader.GetResource(appxApplication.Square150x150Logo);
                            if (stream != null)
                            {
                                using var memStream = new MemoryStream();
                                await stream.CopyToAsync(memStream, cancellationToken).ConfigureAwait(false);
                                memStream.Seek(0, SeekOrigin.Begin);
                                appxApplication.Logo = memStream.ToArray();
                            }
                            else
                            {
                                appxApplication.Logo = appxPackage.Logo;
                            }
                        }
                    }

                    appxPackage.Applications.Add(appxApplication);
                }

                foreach (var psfApp in appxPackage.Applications)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var type = PackageTypeConverter.GetPackageTypeFrom(psfApp.EntryPoint, psfApp.Executable, psfApp.StartPage, appxPackage.IsFramework);

                    switch (type)
                    {
                        case MsixApplicationType.Win32AiStub:
                        case MsixApplicationType.Win32Psf:
                        case MsixApplicationType.MsixHelper:
                            var proxyReader = ApplicationProxyReader.Create(psfApp.Id, psfApp.Executable, fileReader);
                            psfApp.Proxy = await proxyReader.Inspect(cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }
            }

            if (nodeDependenciesRoot != null)
            {
                var dependencies = nodeDependenciesRoot.Elements();

                if (dependencies != null)
                {
                    foreach (var node in dependencies)
                    {
                        switch (node.Name.LocalName)
                        {
                            case "MainPackageDependency":
                                var modName = node.Attribute("Name")?.Value;

                                var appxModPackDependency = new AppxMainPackageDependency
                                {
                                    Name = modName,
                                };

                                appxPackage.MainPackages.Add(appxModPackDependency);

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
                                        Minimum = WindowsNames.GetOperatingSystemFromNameAndVersion(name, minVersion),
                                        Tested = WindowsNames.GetOperatingSystemFromNameAndVersion(name, maxVersion),
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
                }
            }

            if (nodePrerequisitesRoot != null)
            {
                var min = nodePrerequisitesRoot.Element(ns2 + "OSMinVersion")?.Value;
                var max = nodePrerequisitesRoot.Element(ns2 + "OSMaxVersionTested")?.Value;

                appxPackage.OperatingSystemDependencies.Add(new AppxOperatingSystemDependency
                {
                    Minimum = min == null ? null : WindowsNames.GetOperatingSystemFromNameAndVersion("Windows.Desktop", min),
                    Tested = max == null ? null : WindowsNames.GetOperatingSystemFromNameAndVersion("Windows.Desktop", max),
                });
            }

            if (nodeBuild != null)
            {
                var buildKeyValues = new Dictionary<string, string>();

                foreach (var buildNode in nodeBuild.Elements(build + "Item"))
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

            appxPackage.FamilyName = AppxPackagingNameHelper.GetPackageFamilyName(appxPackage.Name, appxPackage.Publisher);
            appxPackage.FullName = AppxPackagingNameHelper.GetPackageFullName(appxPackage.Name, appxPackage.Publisher, appxPackage.ProcessorArchitecture, appxPackage.Version, appxPackage.ResourceId);
            appxPackage.Capabilities = this.GetCapabilities(nodeCapabilitiesRoot);

            return await Translate(fileReader, appxPackage, cancellationToken).ConfigureAwait(false);
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
                    await using (var stream = fileReader.GetFile("resources.pri"))
                    {
                        if (stream is FileStream fileStream)
                        {
                            priFullPath = fileStream.Name;
                        }
                        else
                        {
                            priFullPath = Path.GetTempFileName();
                            cleanup = true;

                            await using var fs = System.IO.File.OpenWrite(priFullPath);
                            await stream.CopyToAsync(fs, cancellationToken).ConfigureAwait(false);
                            await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }

                    var resourceTranslator = new ResourceTranslator(package.FullName, priFullPath);

                    package.DisplayName = resourceTranslator.Translate(package.DisplayName);
                    package.Description = resourceTranslator.Translate(package.Description);
                    package.PublisherDisplayName = resourceTranslator.Translate(package.PublisherDisplayName);

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
                        app.DisplayName = resourceTranslator.Translate(app.DisplayName);

                        if (string.IsNullOrEmpty(app.DisplayName))
                        {
                            app.DisplayName = app.Id;
                        }

                        app.Description = resourceTranslator.Translate(app.Description);
                    }
                }

                return package;
            }
            finally
            {
                if (cleanup && System.IO.File.Exists(priFullPath))
                {
                    System.IO.File.Delete(priFullPath);
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