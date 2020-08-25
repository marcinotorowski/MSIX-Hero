using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017")]
    public class AppInstallerConfig2017 : AppInstallerConfig
    {
        public AppInstallerConfig2017()
        {
        }

        public AppInstallerConfig2017(AppInstallerConfig config) : base(config)
        {
            if (this.UpdateSettings?.OnLaunch != null)
            {
                // Available from 2018/0
                this.UpdateSettings.OnLaunch.UpdateBlocksActivation = false;
                this.UpdateSettings.OnLaunch.ShowPrompt = false;
            }

            if (this.UpdateSettings?.AutomaticBackgroundTask != null)
            {
                // Available from 2017/2
                this.UpdateSettings.AutomaticBackgroundTask = null;
            }

            if (this.UpdateSettings?.ForceUpdateFromAnyVersion != null)
            {
                // Available from 2017/2
                this.UpdateSettings.ForceUpdateFromAnyVersion = false;
            }
        }

        public static Task<AppInstallerConfig2017> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig2017));
                var aic = (AppInstallerConfig2017)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }

    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
    public class AppInstallerConfig20172 : AppInstallerConfig
    {
        public AppInstallerConfig20172()
        {
        }

        public AppInstallerConfig20172(AppInstallerConfig config) : base(config)
        {
            if (this.UpdateSettings?.OnLaunch != null)
            {
                // Available from 2018/0
                this.UpdateSettings.OnLaunch.UpdateBlocksActivation = false;
                this.UpdateSettings.OnLaunch.ShowPrompt = false;
            }
        }

        public static Task<AppInstallerConfig20172> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig20172));
                var aic = (AppInstallerConfig20172)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }

    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2018")]
    public class AppInstallerConfig2018 : AppInstallerConfig
    {
        public AppInstallerConfig2018()
        {
        }
        
        public AppInstallerConfig2018(AppInstallerConfig config) : base(config)
        {
        }

        public static Task<AppInstallerConfig2018> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig2018));
                var aic = (AppInstallerConfig2018)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }

    public class AppInstallerConfig
    {
        public AppInstallerConfig()
        {
        }

        public AppInstallerConfig(AppInstallerConfig config)
        {
            this.MainPackage = config.MainPackage;
            this.MainBundle = config.MainBundle;
            this.Version = config.Version;
            this.Dependencies = config.Dependencies;
            this.Uri = config.Uri;
            this.Optional = config.Optional;
            this.Related = config.Related;
            this.UpdateSettings = config.UpdateSettings;
        }
        
        [XmlElement("MainPackage")]
        public AppInstallerPackageEntry MainPackage { get; set; }

        [XmlElement("MainBundle")]
        public AppInstallerBundleEntry MainBundle { get; set; }

        [XmlAttribute]
        public string Uri { get; set; }

        [XmlAttribute]
        public string Version { get; set; }
            
        /// <summary>
        /// Defines the optional packages that will be installed along with the main package.
        /// </summary>
        [XmlArray("OptionalPackages")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Optional { get; set; }
        
        [XmlArray("RelatedPackages")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Related { get; set; }
            
        [XmlArray("Dependencies")]
        [XmlArrayItem("Bundle", typeof(AppInstallerBundleEntry))]
        [XmlArrayItem("Package", typeof(AppInstallerPackageEntry))]
        public List<AppInstallerBaseEntry> Dependencies { get; set; }
            
        [XmlElement("UpdateSettings")]
        public UpdateSettings UpdateSettings { get; set; }

        public static async Task<AppInstallerConfig> FromFile(string filePath)
        {
            var allText = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);

            var hasBom = allText.Take(3).Select(b => (ushort)b).SequenceEqual(new[] {(ushort)0xEF, (ushort)0xBB, (ushort)0xBF });
            if (hasBom)
            {
                allText = allText.Skip(3).ToArray();
            }

            var xml = System.Text.Encoding.UTF8.GetString(allText);

            var doc = XDocument.Parse(xml);
            if (doc.Root == null)
            {
                throw new FormatException("XML file does not have a root element.");
            }
            if (string.IsNullOrEmpty(doc.Root.Name.NamespaceName) || !doc.Root.Name.NamespaceName.StartsWith("http://schemas.microsoft.com/appx/appinstaller/"))
            {
                throw new FormatException("Root element must have appinstaller namespace.");
            }

            using (var memStream = new MemoryStream(allText))
            {
                switch (doc.Root.Name.NamespaceName)
                {
                    case "http://schemas.microsoft.com/appx/appinstaller/2017":
                        return await AppInstallerConfig2017.FromStream(memStream).ConfigureAwait(false);
                    case "http://schemas.microsoft.com/appx/appinstaller/2017/2":
                        return await AppInstallerConfig20172.FromStream(memStream).ConfigureAwait(false);
                    case "http://schemas.microsoft.com/appx/appinstaller/2018":
                        return await AppInstallerConfig2018.FromStream(memStream).ConfigureAwait(false);
                    default:
                        throw new FormatException("Namespace " + doc.Root.Name.NamespaceName + " is not supported.");
                }
            }
        }
    }
}
