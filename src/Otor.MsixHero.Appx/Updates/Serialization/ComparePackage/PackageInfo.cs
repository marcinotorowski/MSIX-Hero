using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class PackageInfo
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Version { get; set; }
    }
}