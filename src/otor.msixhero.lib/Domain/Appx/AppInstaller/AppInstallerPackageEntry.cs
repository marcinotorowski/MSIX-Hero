using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    [Serializable]
    public class AppInstallerPackageEntry : AppInstallerBaseEntry
    {
        [XmlAttribute("PackageArchitecture")]
        public AppInstallerPackageArchitecture Architecture { get; set;  }
    }
}