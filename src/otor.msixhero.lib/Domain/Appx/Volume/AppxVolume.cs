using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.Volume
{
    [Serializable]
    public class AppxVolume
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string PackageStorePath { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }
    }
}
