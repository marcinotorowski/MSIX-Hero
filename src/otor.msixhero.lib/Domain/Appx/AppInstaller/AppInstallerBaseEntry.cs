using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    [Serializable]
    [KnownType(typeof(AppInstallerBundleEntry))]
    [KnownType(typeof(AppInstallerPackageEntry))]
    public abstract class AppInstallerBaseEntry
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Publisher { get; set; }

        [XmlAttribute]
        public string Version { get; set; }

        [XmlAttribute]
        public string Uri { get; set; }
    }
}