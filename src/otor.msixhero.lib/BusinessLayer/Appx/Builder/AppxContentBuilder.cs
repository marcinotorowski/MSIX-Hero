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
            
            var sb = new StringBuilder();
            await using var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, config);
            await File.WriteAllTextAsync(file, textWriter.ToString(), cancellationToken).ConfigureAwait(false);
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
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(modPackageTemplate);
            this.PrepareModificationPackage(xmlDoc, config);

            var manifestContent = xmlDoc.OuterXml;
            if (action == ModificationPackageBuilderAction.Manifest)
            {
                await File.WriteAllTextAsync(filePath, manifestContent, Encoding.UTF8, cancellation).ConfigureAwait(false);
                return;
            }

            var tempFolder = Environment.ExpandEnvironmentVariables(@"%temp%\msix-hero-" + Guid.NewGuid().ToString("N").Substring(10));
            try
            {
                Directory.CreateDirectory(tempFolder);
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
            Logger.Trace("Executing XQuery /*[local-name()='Package']/*[local-name()='Dependencies'] for a single node...");
            var package = template.SelectSingleNode("/*[local-name()='Package']");
            if (package == null)
            {
                package = template.CreateElement(null, "Package", null);
                template.AppendChild(package);
            }

            var dependencies = package.SelectSingleNode("/*[local-name()='Dependencies']");
            if (dependencies == null)
            {
                dependencies = template.CreateElement(null, "Dependencies", null);
                package.AppendChild(dependencies);
            }

            var dependency = template.CreateElement("uap4", "MainPackageDependency", "http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            dependencies.AppendChild(dependency);

            dependency.SetAttribute("Name", config.ParentName);
            dependency.SetAttribute("Publisher", config.ParentPublisher);

            var identity = package.SelectSingleNode("/*[local-name()='Identity']") as XmlElement;
            if (identity == null)
            {
                identity = template.CreateElement(null, "Identity", null);
                package.AppendChild(identity);
            }

            identity.SetAttribute("Name", config.Name);
            identity.SetAttribute("ProcessorArchitecture", config.Architecture.ToString());
            identity.SetAttribute("Publisher", config.Publisher);
            identity.SetAttribute("Version", config.Version);
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