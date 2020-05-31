using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage
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