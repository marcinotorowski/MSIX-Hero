using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    [Serializable]
    public class AppInstallerPackageEntry : AppInstallerBaseEntry
    {
        [XmlAttribute]
        public AppInstallerPackageArchitecture Architecture { get; }
    }
}