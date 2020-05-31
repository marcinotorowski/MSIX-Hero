using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage
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
