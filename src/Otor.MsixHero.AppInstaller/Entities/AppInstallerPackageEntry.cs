using System;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    public class AppInstallerPackageEntry : AppInstallerBaseEntry
    {
        [XmlAttribute("ProcessorArchitecture")]
        public AppInstallerPackageArchitecture Architecture { get; set;  }
    }
}