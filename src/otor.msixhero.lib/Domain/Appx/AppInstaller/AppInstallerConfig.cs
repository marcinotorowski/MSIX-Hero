using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using NLog.Layouts;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
    public class AppInstallerConfig
    {
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
    }
}
