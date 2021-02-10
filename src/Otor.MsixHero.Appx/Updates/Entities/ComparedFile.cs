using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparedFile
    {
        public ComparedFile(string name, long uncompressedSize, IEnumerable<ComparedBlock> blocks = null)
        {
            this.Name = name;
            this.UncompressedSize = uncompressedSize;
            this.Blocks = blocks == null ? new List<ComparedBlock>() : new List<ComparedBlock>(blocks);
        }

        public string Name { get; }

        public long UncompressedSize { get; }

        public IList<ComparedBlock> Blocks { get; }

        public long UpdateImpact { get; set; }

        public long SizeDifference { get; set; }

        public ComparisonStatus Status { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}