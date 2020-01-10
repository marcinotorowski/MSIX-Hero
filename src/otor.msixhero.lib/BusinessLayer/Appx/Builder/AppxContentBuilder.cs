using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ControlzEx.Standard;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.ModificationPackage;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Builder
{
    public class AppxContentBuilder : IAppxContentBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private const string DefaultNamespacePrefix = "msixHero";

        private readonly IAppxPacker packer;
        private readonly IAppxSigningManager signingManager;

        public AppxContentBuilder(IAppxPacker packer, IAppxSigningManager signingManager)
        {
            this.packer = packer;
            this.signingManager = signingManager;
        }

        public async Task Create(AppInstallerConfig config, string file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(file);
            // ReSharper disable once PossibleNullReferenceException
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig));
            
            using (var textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, config);

                var ns = this.GetMinimumSupportedWindowsVersion(config).Item2;

                var content = textWriter.ToString().Replace("http://schemas.microsoft.com/appx/appinstaller/2017", ns);
                
                await File.WriteAllTextAsync(file, content, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
            }
        }

        public Tuple<int, string> GetMinimumSupportedWindowsVersion(AppInstallerConfig config)
        {
            var maximum = 1709;
            var maximumNamespace = 20170;

            if (config.UpdateSettings != null)
            {
                if (config.UpdateSettings.ForceUpdateFromAnyVersion)
                {
                    maximum = Math.Max(maximum, 1809);
                    maximumNamespace = Math.Max(maximumNamespace, 20172);
                }

                if (config.UpdateSettings.AutomaticBackgroundTask != null)
                {
                    maximum = Math.Max(maximum, 1803);
                    maximumNamespace = Math.Max(maximumNamespace, 20172);
                }

                if (config.UpdateSettings.OnLaunch != null)
                {
                    if (config.UpdateSettings.OnLaunch.UpdateBlocksActivation || config.UpdateSettings.OnLaunch.ShowPrompt)
                    {
                        maximum = Math.Max(maximum, 1903);
                        maximumNamespace = Math.Max(maximumNamespace, 20180);
                    }
                }
            }

            var ns = "http://schemas.microsoft.com/appx/appinstaller/";
            if (maximumNamespace % 10 == 0)
            {
                ns += (maximumNamespace / 10);
            }
            else
            {
                var minor = maximumNamespace % 10;
                var major = (maximumNamespace - minor) / 10;

                ns += major;
                if (minor != 0)
                {
                    ns += "/" + minor;
                }
            }
            return new Tuple<int, string>(maximum, ns);
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
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(modPackageTemplate);
            this.PrepareModificationPackage(xmlDoc, config);

            var manifestContent = xmlDoc.OuterXml;

            if (action == ModificationPackageBuilderAction.Manifest)
            {
                using (var sourceStream = File.OpenRead(logoPath))
                {
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

                await File.WriteAllTextAsync(Path.Join(tempFolder, "AppxManifest.xml"), manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);
                await this.packer.Pack(tempFolder, filePath, true, cancellation, progress).ConfigureAwait(false);
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

            switch (action)
            {
                case ModificationPackageBuilderAction.Manifest:
                case ModificationPackageBuilderAction.Msix:
                    break;
                case ModificationPackageBuilderAction.SignedMsix:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        private void PrepareModificationPackage(XmlDocument template, ModificationPackageConfig config)
        {
            var namespaceManager = new XmlNamespaceManager(template.NameTable);
            if (!namespaceManager.HasNamespace("uap4"))
            {
                namespaceManager.AddNamespace("uap4", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            }

            if (!namespaceManager.HasNamespace("build"))
            {
                namespaceManager.AddNamespace("build", "http://schemas.microsoft.com/developer/appx/2015/build");
            }

            namespaceManager.AddNamespace(DefaultNamespacePrefix, template.DocumentElement.NamespaceURI);

            var package = GetOrCreateNode(template, template, "Package", DefaultNamespacePrefix, namespaceManager);
            var dependencies = GetOrCreateNode(template, package, "Dependencies", DefaultNamespacePrefix, namespaceManager);
            var dependency = template.CreateElement("uap4", "MainPackageDependency", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            dependencies.AppendChild(dependency);
            dependency.SetAttribute("Name", config.ParentName);
            dependency.SetAttribute("Publisher", config.ParentPublisher);

            var identity = GetOrCreateNode(template, package, "Identity", DefaultNamespacePrefix, namespaceManager);
            identity.SetAttribute("Name", config.Name);
            identity.SetAttribute("ProcessorArchitecture", config.Architecture.ToString());
            identity.SetAttribute("Publisher", config.Publisher);

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

            identity.SetAttribute("Version", new Version(major, minor, build, revision).ToString(4));

            var properties = GetOrCreateNode(template, package, "Properties", DefaultNamespacePrefix, namespaceManager);
            GetOrCreateNode(template, properties, "DisplayName", DefaultNamespacePrefix, namespaceManager).InnerText = "Modification Package Name";
            GetOrCreateNode(template, properties, "PublisherDisplayName", DefaultNamespacePrefix, namespaceManager).InnerText = "Modification Package Publisher Name";
            GetOrCreateNode(template, properties, "Description", DefaultNamespacePrefix, namespaceManager).InnerText = "Modification Package Description";
            GetOrCreateNode(template, properties, "Logo", DefaultNamespacePrefix, namespaceManager).InnerText = "Assets\\Logo.png";
            
            var metaData = GetNode(template, package, "build:Metadata", namespaceManager);
            if (metaData != null)
            {
                package.RemoveChild(metaData);
            }

            metaData = CreateNode(template, package, "build:Metadata", namespaceManager);

            var version = NtDll.RtlGetVersion();

            var operatingSystem = CreateNode(template, metaData, "build:Item", namespaceManager);
            operatingSystem.SetAttribute("Name", "OperatingSystem");
            operatingSystem.SetAttribute("Version", $"{version.ToString(4)}");

            var msixHero = CreateNode(template, metaData, "build:Item", namespaceManager);
            msixHero.SetAttribute("Name", "MsixHero");
            msixHero.SetAttribute("Version", (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly()).GetName().Version.ToString());

            var signTool = CreateNode(template, metaData, "build:Item", namespaceManager);
            signTool.SetAttribute("Name", "SignTool.exe");
            signTool.SetAttribute("Version", GetVersion("SignTool.exe"));

            var makepri = CreateNode(template, metaData, "build:Item", namespaceManager);
            makepri.SetAttribute("Name", "MakePri.exe");
            makepri.SetAttribute("Version", GetVersion("MakePri.exe"));

            var makeappx = CreateNode(template, metaData, "build:Item", namespaceManager);
            makeappx.SetAttribute("Name", "MakeAppx.exe");
            makeappx.SetAttribute("Version", GetVersion("MakeAppx.exe"));
        }

        private static string GetVersion(string sdkFile)
        {
            var path = MsixSdkWrapper.GetSdkPath(sdkFile);
            if (!File.Exists(path))
            {
                return null;
            }

            return FileVersionInfo.GetVersionInfo(path).ProductVersion;
        }

        private static XmlElement GetNode(XmlDocument xmlDocument, XmlNode xmlNode, string qualifiedName, XmlNamespaceManager namespaceManager)
        {
            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            var indexOf = qualifiedName.IndexOf(':');
            if (indexOf == -1)
            {
                return GetNode(xmlDocument, xmlNode, qualifiedName, DefaultNamespacePrefix, namespaceManager);
            }

            return GetNode(xmlDocument, xmlNode, qualifiedName.Substring(indexOf + 1), qualifiedName.Substring(0, indexOf), namespaceManager);
        }

        private static XmlElement GetOrCreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string qualifiedName, XmlNamespaceManager namespaceManager)
        {
            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            var indexOf = qualifiedName.IndexOf(':');
            if (indexOf == -1)
            {
                return GetOrCreateNode(xmlDocument, xmlNode, qualifiedName, DefaultNamespacePrefix, namespaceManager);
            }

            return GetOrCreateNode(xmlDocument, xmlNode, qualifiedName.Substring(indexOf + 1), qualifiedName.Substring(0, indexOf), namespaceManager);
        }

        private static XmlElement CreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string qualifiedName, XmlNamespaceManager namespaceManager)
        {
            if (string.IsNullOrEmpty(qualifiedName))
            {
                throw new ArgumentNullException(nameof(qualifiedName));
            }

            var indexOf = qualifiedName.IndexOf(':');
            if (indexOf == -1)
            {
                return CreateNode(xmlDocument, xmlNode, qualifiedName, DefaultNamespacePrefix, namespaceManager);
            }

            return CreateNode(xmlDocument, xmlNode, qualifiedName.Substring(indexOf + 1), qualifiedName.Substring(0, indexOf), namespaceManager);
        }

        private static XmlElement GetNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            return (XmlElement)node;
        }

        private static XmlElement GetOrCreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            // ReSharper disable once InvertIf
            if (node == null)
            {
                return CreateNode(xmlDocument, xmlNode, name, prefix, namespaceManager);
            }

            return (XmlElement)node;
        }

        private static XmlElement CreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var ns = namespaceManager.LookupNamespace(prefix);
            if (string.IsNullOrEmpty(ns))
            {
                ns = xmlDocument.DocumentElement.NamespaceURI;
            }

            if (ns != xmlDocument.DocumentElement.NamespaceURI && !string.IsNullOrEmpty(prefix))
            {
                name = $"{prefix}:{name}";
            }

            var node = xmlDocument.CreateElement(name, ns);
            xmlNode.AppendChild(node);
            return (XmlElement)node;
        }

        private static string GetBundledResourcePath(string localName)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates", localName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not locale resource {path}.", path);
            }

            return path;
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter()
            {
            }

            public Utf8StringWriter(IFormatProvider formatProvider) : base(formatProvider)
            {
            }

            public Utf8StringWriter(StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider)
            {
            }

            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}