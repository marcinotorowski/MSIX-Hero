using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class File : ISize, IUpdateImpact
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public long Size { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }
    }
}