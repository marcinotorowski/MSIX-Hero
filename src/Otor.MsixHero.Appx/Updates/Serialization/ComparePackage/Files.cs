using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class Files : ISize, IUpdateImpact
    {
        [XmlAttribute]
        public long Size { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }

        [XmlElement(ElementName = "File")]
        public List<File> Items { get; set; }
    }
}