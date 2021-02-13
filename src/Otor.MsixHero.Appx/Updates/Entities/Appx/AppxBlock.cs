using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    [Serializable]
    public class AppxBlock
    {
        public AppxBlock()
        {
        }

        public AppxBlock(string hash, long compressedSize)
        {
            this.Hash = hash;
            this.CompressedSize = compressedSize;
        }

        [XmlAttribute("hash")]
        public string Hash { get; set; }

        [XmlAttribute("size")]
        public long CompressedSize { get; set; }

        [XmlAttribute("type")]
        public ComparisonStatus Status { get; set; }

        [XmlAttribute("updateImpact")]
        public long UpdateImpact { get; set; }

        public override string ToString()
        {
            return this.Hash;
        }
    }
}