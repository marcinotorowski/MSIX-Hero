using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage
{
    public class PackageInfo
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Version { get; set; }
    }
}