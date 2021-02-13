using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public abstract class BaseDuplicateFiles
    {
        [XmlAttribute("possibleSizeReduction")]
        public long PossibleSizeReduction { get; set; }

        [XmlAttribute("possibleImpactReduction")]
        public long PossibleImpactReduction { get; set; }
    }
}