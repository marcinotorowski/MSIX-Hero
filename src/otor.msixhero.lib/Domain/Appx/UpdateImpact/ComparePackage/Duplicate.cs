using System.Collections.Generic;
using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage
{
    public class Duplicate : IPossibleImpactReduction, IPossibleSizeReduction
    {
        [XmlAttribute]
        public long PossibleSizeReduction { get; set; }

        [XmlAttribute]
        public long PossibleImpactReduction { get; set; }

        [XmlElement(ElementName = "File")]
        public List<DuplicateFile> Items { get; set; }
    }
}