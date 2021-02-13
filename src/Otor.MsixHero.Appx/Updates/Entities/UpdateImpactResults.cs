using System.IO;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Entities.Appx;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    [XmlRoot("updateImpact")]
    public class UpdateImpactResults
    {
        [XmlAttribute("oldPackage")]
        public string OldPackage { get; set; }
        
        [XmlAttribute("newPackage")]
        public string NewPackage { get; set; }

        [XmlElement("added")]
        public AddedFiles AddedFiles { get; set; }

        [XmlElement("deleted")]
        public DeletedFiles DeletedFiles { get; set; }

        [XmlElement("changed")]
        public ChangedFiles ChangedFiles { get; set; }

        [XmlElement("unchanged")]
        public UnchangedFiles UnchangedFiles { get; set; }
        
        [XmlAttribute("updateImpact")]
        public long UpdateImpact { get; set; }

        [XmlIgnore]
        public long ActualUpdateImpact { get; set; }

        [XmlAttribute("sizeDiff")]
        public long SizeDifference { get; set; }

        [XmlElement("oldPackageLayout")]
        public AppxLayout OldPackageLayout { get; set; }

        [XmlElement("newPackageLayout")]
        public AppxLayout NewPackageLayout { get; set; }

        [XmlElement("oldPackageDuplication")]
        public AppxDuplication OldPackageDuplication { get; set; }

        [XmlElement("newPackageDuplication")]
        public AppxDuplication NewPackageDuplication { get; set; }

        public void Export(Stream outputStream)
        {
            var xmlSerializer = new XmlSerializer(typeof(UpdateImpactResults));
            xmlSerializer.Serialize(outputStream, this);
        }

        public void Export(FileInfo file)
        {
            if (file.Exists)
            {
                file.Delete();
            }    
            else if (file.Directory?.Exists == false)
            {
                file.Directory.Create();
            }

            using var fs = File.OpenWrite(file.FullName);
            this.Export(fs);
        }
    }
}