using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class DuplicateFile : ISize, IDuplicateImpact
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public long Size { get; set; }

        [XmlAttribute]
        public long DuplicateImpact { get; set; }
    }
}