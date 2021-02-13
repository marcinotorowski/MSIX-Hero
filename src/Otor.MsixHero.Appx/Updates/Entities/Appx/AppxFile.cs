using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    [Serializable]
    [XmlRoot("file")]
    public class AppxFile
    {
        public AppxFile()
        {
        }

        public AppxFile(string name, long uncompressedSize, ushort headerSize = 30, IEnumerable<AppxBlock> blocks = null)
        {
            this.Name = name;
            this.UncompressedSize = uncompressedSize;
            this.HeaderSize = headerSize;
            this.Blocks = blocks == null ? new List<AppxBlock>() : new List<AppxBlock>(blocks);
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("headerSize")]
        public ushort HeaderSize { get; set; }

        [XmlAttribute("size")]
        public long UncompressedSize { get; set; }

        [XmlElement("block")]
        public List<AppxBlock> Blocks { get; set; }

        [XmlAttribute("updateImpact")]
        public long UpdateImpact { get; set; }

        [XmlAttribute("sizeDiff")]
        public long SizeDifference { get; set; }

        [XmlAttribute("type")]
        public ComparisonStatus Status { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}