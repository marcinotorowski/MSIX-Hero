using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Appx.Packer;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.ModificationPackage;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Builder
{
    public class AppxContentBuilder : IAppxContentBuilder
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly IAppxPacker packer;

        public AppxContentBuilder(IAppxPacker packer)
        {
            this.packer = packer;
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
            
            using (var textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, config);
                await File.WriteAllTextAsync(file, textWriter.ToString(), cancellationToken).ConfigureAwait(false);
            }
        }

        public int GetMinimumSupportedWindowsVersion(AppInstallerConfig config)
        {
            var maximum = 1709;

            if (config.UpdateSettings != null)
            {
                if (config.UpdateSettings.ForceUpdateFromAnyVersion)
                {
                    maximum = Math.Max(maximum, 1809);
                }

                if (config.UpdateSettings.AutomaticBackgroundTask != null)
                {
                    maximum = Math.Max(maximum, 1803);
                }

                if (config.UpdateSettings.OnLaunch != null)
                {
                    if (config.UpdateSettings.OnLaunch.UpdateBlocksActivation || config.UpdateSettings.OnLaunch.ShowPrompt)
                    {
                        maximum = Math.Max(maximum, 1903);
                    }
                }
            }

            return maximum;
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
                await this.packer.Pack(tempFolder, filePath, cancellation, progress).ConfigureAwait(false);

                if (action == ModificationPackageBuilderAction.SignedMsix)
                {
                    // todo: signing
                }
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

            const string DefaultNamespacePrefix = "msixHero";
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
        }

        private static XmlElement GetOrCreateNode(XmlDocument xmlDocument, XmlNode xmlNode, string name, string prefix, XmlNamespaceManager namespaceManager)
        {
            var node = xmlNode.SelectSingleNode($"{prefix}:{name}", namespaceManager);
            if (node == null)
            {
                node = xmlDocument.CreateElement(name, xmlDocument.DocumentElement.NamespaceURI);
                xmlNode.AppendChild(node);
            }

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
    }
}