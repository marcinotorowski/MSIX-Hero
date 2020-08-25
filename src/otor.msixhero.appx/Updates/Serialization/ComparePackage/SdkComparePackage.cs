using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Updates.Serialization.ComparePackage
{
    [XmlRoot("ComparePackage")]
    public class SdkComparePackage
    {
        [XmlElement]
        public PackageInfo Original { get; set; }

        [XmlElement]
        public PackageInfo New { get; set; }

        [XmlElement]
        public PackageDifference Package { get; set; }
    }
}
