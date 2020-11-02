using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class ChangedFiles : ISizeDifference, IUpdateImpact
    {
        [XmlAttribute]
        public long SizeDifference { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }

        [XmlElement(ElementName = "File")]
        public List<ChangedFile> Items { get; set; }
    }
}