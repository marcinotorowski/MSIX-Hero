using System.Collections.Generic;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class DuplicateFiles : IPossibleSizeReduction, IPossibleImpactReduction
    {
        [XmlAttribute]
        public long PossibleSizeReduction { get; set; }

        [XmlAttribute]
        public long PossibleImpactReduction { get; set; }

        [XmlElement(ElementName = "Duplicate")]
        public List<Duplicate> Items { get; set; }
    }
}