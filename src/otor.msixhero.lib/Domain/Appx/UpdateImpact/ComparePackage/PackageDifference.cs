using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage
{
    public class PackageDifference : ISizeDifference, IAddedSize, IDeletedSize, IUpdateImpact, ISize
    {
        [XmlAttribute]
        public long SizeDifference { get; set; }

        [XmlAttribute]
        public long AddedSize { get; set; }

        [XmlAttribute]
        public long DeletedSize { get; set; }

        [XmlAttribute]
        public long UpdateImpact { get; set; }

        [XmlAttribute]
        public long Size { get; set; }

        [XmlElement]
        public ChangedFiles ChangedFiles { get; set; }

        [XmlElement]
        public Files AddedFiles { get; set; }

        [XmlElement]
        public Files DeletedFiles { get; set; }

        [XmlElement]
        public Files UnchangedFiles { get; set; }

        [XmlElement]
        public DuplicateFiles DuplicateFiles { get; set; }
    }
}