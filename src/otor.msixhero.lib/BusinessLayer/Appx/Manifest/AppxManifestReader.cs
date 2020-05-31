using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Helpers;
using otor.msixhero.lib.Domain.Appx.Manifest.Build;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Interop;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public class AppxManifestReader : IAppxManifestReader
    {
        protected readonly PsfReader PsfReader = new PsfReader();
        
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AppxManifestReader));

        public Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            var isMsix = fileReader.FileExists("AppxManifest.xml");
            if (isMsix)
            {
                return this.ReadMsix(fileReader, cancellationToken);
            }

            var isAppxBundle = fileReader.FileExists("AppxMetaData\\AppxBundleManifest.xml");
            if (isAppxBundle)
            {
                throw new NotImplementedException("Reading of bundle manifests is not supported yet.");
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
        
        private async Task<AppxPackage> ReadMsix(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            using (var file = fileReader.GetFile("AppxManifest.xml"))
            {
                var document = await XDocument.LoadAsync(file, LoadOptions.None, cancellationToken).ConfigureAwait(false);
                if (document.Root == null)
                {
                    throw new FormatException("The manifest is malformed. There is no root element.");
                }

                var ns = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/foundation/windows10");
                var uap = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10");
                var uap10 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/10");
                var desktop6 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/6");
                var desktop2 = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/desktop/windows10/2");

                if (document.Root == null)
                {
                    throw new FormatException("The manifest is malformed. The document root must be <Package /> element.");
                }

                var nodePackage = document.Root.Name.LocalName == "Package" && document.Root.Name.Namespace == ns ? document.Root : null;

                if (nodePackage == null)
                {
                    throw new FormatException("The manifest is malformed. The document root must be <Package /> element.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                var nodeIdentity = nodePackage.Element(ns + "Identity");

                cancellationToken.ThrowIfCancellationRequested();
                var nodeProperties = nodePackage.Element(ns + "Properties");

                cancellationToken.ThrowIfCancellationRequested();
                var nodeApplicationsRoot = nodePackage.Element(ns + "Applications");

                cancellationToken.ThrowIfCancellationRequested();
                var nodeCapabilitiesRoot = nodePackage.Element(ns + "Capabilities");

                cancellationToken.ThrowIfCancellationRequested();
                var nodeDependenciesRoot = nodePackage.Element(ns + "Dependencies");

                cancellationToken.ThrowIfCancellationRequested();
                var nodeBuild = nodePackage.Element(ns + "Metadata");

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

                cancellationToken.ThrowIfCancellationRequested();
                if (nodeProperties != null)
                {
                    foreach (var node in nodeProperties.Elements())
                    {
                        switch (node.Name.LocalName)
                        {
                            case "Logo":
                                var logo = node.Value;
                                if (!string.IsNullOrEmpty(logo))
                                {
                                    using (var resourceStream = fileReader.GetResource(logo))
                                    {
                                        if (resourceStream != null)
                                        {
                                            using (var memoryStream = new MemoryStream())
                                            {
                                                await resourceStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                                                await memoryStream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                                appxPackage.Logo = memoryStream.ToArray();
                                            }
                                        }
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

                cancellationToken.ThrowIfCancellationRequested();
                appxPackage.PackageDependencies = new List<AppxPackageDependency>();
                appxPackage.OperatingSystemDependencies = new List<AppxOperatingSystemDependency>();
                appxPackage.Applications = new List<AppxApplication>();

                if (nodeApplicationsRoot != null)
                {
                    foreach (var node in nodeApplicationsRoot.Elements(ns + "Application"))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        /*
                             *<Application EntryPoint="SparklerApp.App" Executable="XD.exe" Id="App">
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
                             *
                             */

                        var appxApplication = new AppxApplication();

                        appxApplication.EntryPoint = node.Attribute("EntryPoint")?.Value;
                        appxApplication.StartPage = node.Attribute("StartPage")?.Value;
                        appxApplication.Executable = node.Attribute("Executable")?.Value;
                        appxApplication.Id = node.Attribute("Id")?.Value;

                        /*
                             *<Extensions><desktop6:Extension Category="windows.service" Executable="VFS\ProgramFilesX86\RayPackStudio\FloatingLicenseServer\FloatingLicenseServer.exe" EntryPoint="Windows.FullTrustApplication"><desktop6:Service Name="PkgSuiteFloatingLicenseServer" StartupType="auto" StartAccount="networkService" /></desktop6:Extension></Extensions>
                             *
                             */

                        appxApplication.Extensions = new List<AppxExtension>();
                        var extensions = node.Elements(ns + "Extensions").ToList();

                        foreach (var extension in extensions.Elements(ns + "Extension").Union(extensions.Elements(desktop2 + "Extension").Union(extensions.Elements(desktop6 + "Extension"))))
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var category = extension.Attribute("Category")?.Value;
                            if (category != "windows.service")
                            {
                                continue;
                            }

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
                        }

                        var visualElements = node.Element(uap + "VisualElements");
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
                                using (var stream =
                                    fileReader.GetResource(appxApplication.Square44x44Logo) ??
                                    fileReader.GetResource(appxApplication.Square30x30Logo) ??
                                    fileReader.GetResource(appxApplication.Square71x71Logo) ??
                                    fileReader.GetResource(appxApplication.Square150x150Logo))
                                {
                                    if (stream != null)
                                    {
                                        var bytes = new byte[stream.Length];
                                        await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                                        appxApplication.Logo = bytes;
                                    }
                                    else
                                    {
                                        appxApplication.Logo = appxPackage.Logo;
                                    }
                                }
                            }
                        }

                        appxPackage.Applications.Add(appxApplication);
                    }

                    var psfApps = appxPackage.Applications.Where(a =>
                        PackageTypeConverter.GetPackageTypeFrom(a.EntryPoint, a.Executable, a.StartPage, appxPackage.IsFramework) ==
                        MsixPackageType.BridgePsf).ToArray();

                    if (psfApps.Any())
                    {
                        foreach (var psfApp in psfApps)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            psfApp.Psf = this.PsfReader.Read(psfApp.Id, psfApp.Executable, fileReader);
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
                                case "TargetDeviceFamily":
                                {
                                    /*
                                         * <TargetDeviceFamily MaxVersionTested="10.0.15063.0" MinVersion="10.0.15063.0" Name="Windows.Universal" />
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
                                         * <PackageDependency MinVersion="1.4.24201.0" Name="Microsoft.NET.Native.Runtime.1.4" Publisher="CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US" />
                                         */

                                    var minVersion = node.Attribute("MinVersion")?.Value;
                                    var name = node.Attribute("Name")?.Value;
                                    var publisher = node.Attribute("Publisher")?.Value;

                                    var appxDepdendency = new AppxPackageDependency
                                    {
                                        Publisher = publisher,
                                        Name = name,
                                        Version = minVersion
                                    };

                                    appxPackage.PackageDependencies.Add(appxDepdendency);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (nodeBuild != null)
                {
                    var buildKeyValues = new Dictionary<string, string>();

                    foreach (var buildNode in nodeBuild.Elements(ns + "Item"))
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

                    if (this.DetectAdvancedInstaller(buildKeyValues, out var buildInfo)
                        || this.DetectVisualStudio(buildKeyValues, out buildInfo)
                        || this.DetectRayPack(buildKeyValues, fileReader, out buildInfo)
                        || this.DetectMsixHero(buildKeyValues, out buildInfo))
                    {
                        appxPackage.BuildInfo = buildInfo;
                    }
                }

                appxPackage.FamilyName = AppxPackaging.GetPackageFamilyName(appxPackage.Name, appxPackage.Publisher);
                appxPackage.FullName = AppxPackaging.GetPackageFullName(appxPackage.Name, appxPackage.Publisher, appxPackage.ProcessorArchitecture, appxPackage.Version, appxPackage.ResourceId);
                
                appxPackage.Capabilities = this.GetCapabilities(nodeCapabilitiesRoot);

                return await Translate(fileReader, appxPackage, cancellationToken).ConfigureAwait(false);
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

                    package.DisplayName = StringLocalizer.Localize(priFullPath, package.Name, package.DisplayName);
                    package.Description = StringLocalizer.Localize(priFullPath, package.Name, package.Description);
                    package.PublisherDisplayName = StringLocalizer.Localize(priFullPath, package.Name, package.PublisherDisplayName);

                    foreach (var app in package.Applications ?? Enumerable.Empty<AppxApplication>())
                    {
                        app.DisplayName = StringLocalizer.Localize(priFullPath, app.Id, app.DisplayName);
                        app.Description = StringLocalizer.Localize(priFullPath, app.Id, app.Description);
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

        private bool DetectVisualStudio(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("VisualStudio", out var visualStudio))
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "Microsoft Visual Studio",
                ProductVersion = visualStudio,

            };

            if (buildValues.TryGetValue("OperatingSystem", out var win10))
            {
                var firstUnit = win10.Split(' ')[0];
                buildInfo.OperatingSystem = Windows10Parser.GetOperatingSystemFromNameAndVersion(firstUnit).ToString();
            }

            buildInfo.Components = buildValues;
            return true;
        }

        private bool DetectAdvancedInstaller(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("AdvancedInstaller", out var advInst))
            {
                return false;
            }

            buildValues.TryGetValue("ProjectLicenseType", out var projLic);
            buildInfo = new BuildInfo
            {
                ProductLicense = projLic,
                ProductName = "Advanced Installer",
                ProductVersion = advInst
            };

            if (buildValues.TryGetValue("OperatingSystem", out var os))
            {
                var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version.ToString();
            }

            return true;
        }

        private bool DetectMsixHero(Dictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("MsixHero", out var msixHero))
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "MSIX Hero",
                ProductVersion = msixHero
            };

            if (buildValues.TryGetValue("OperatingSystem", out var os))
            {
                var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version.ToString();
            }

            return true;
        }

        private bool DetectRayPack(Dictionary<string, string> buildValues, IAppxFileReader fileReader, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                // Detect RayPack by taking a look at the metadata of PsfLauncher.
                const string fileLauncher = "PsfLauncher.exe";
                if (fileReader.FileExists(fileLauncher))
                {
                    using (var launcher = fileReader.GetFile(fileLauncher))
                    {
                        FileVersionInfo fileVersionInfo;
                        if (launcher is FileStream fileStream)
                        {
                            fileVersionInfo = FileVersionInfo.GetVersionInfo(fileStream.Name);
                        }
                        else
                        {
                            var tempFilePath = Path.GetTempFileName();
                            try
                            {
                                using (var fs = File.OpenWrite(tempFilePath))
                                {
                                    launcher.CopyTo(fs);
                                    fs.Flush();
                                }

                                fileVersionInfo = FileVersionInfo.GetVersionInfo(tempFilePath);
                            }
                            finally
                            {
                                File.Delete(tempFilePath);
                            }
                        }
                        
                        if (fileVersionInfo.ProductName != null && fileVersionInfo.ProductName.StartsWith("Raynet", StringComparison.OrdinalIgnoreCase))
                        {
                            var pv = fileVersionInfo.ProductVersion;
                            buildInfo = new BuildInfo
                            {
                                ProductName = "RayPack " + Version.Parse(pv).ToString(2),
                                ProductVersion = fileVersionInfo.ProductVersion
                            };

                            return true;
                        }
                    }
                }
            }
            else
            {
                // Detect RayPack 6.2 which uses build meta data like this:
                // <build:Item Name="OperatingSystem" Version="6.2.9200.0" /><build:Item Name="Raynet.RaySuite.Common.Appx" Version="6.2.5306.1168" /></build:Metadata>
                if (buildValues.TryGetValue("Raynet.RaySuite.Common.Appx", out var rayPack))
                {
                    if (Version.TryParse(rayPack, out var parsedVersion))
                    {
                        buildInfo = new BuildInfo
                        {
                            ProductName = $"RayPack {parsedVersion.Major}.{parsedVersion.Minor}",
                            ProductVersion = $"(MSIX builder v{parsedVersion})"
                        };
                    }
                    else
                    {
                        buildInfo = new BuildInfo
                        {
                            ProductName = "RayPack",
                            ProductVersion = $"(MSIX builder v{rayPack})"
                        };
                    }

                    if (buildValues.TryGetValue("OperatingSystem", out var os))
                    {
                        var win10Version = Windows10Parser.GetOperatingSystemFromNameAndVersion(os);
                        buildInfo.OperatingSystem = win10Version.ToString();
                    }

                    return true;
                }
            }

            return false;
        }
    }
}