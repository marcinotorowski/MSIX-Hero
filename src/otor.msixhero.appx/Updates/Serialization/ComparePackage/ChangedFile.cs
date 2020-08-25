using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    public class ChangedFile : ISizeDifference, IUpdateImpact
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public long SizeDifference { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }
    }
}