using System.Collections.Generic;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    public class AppxFile
    {
        public AppxFile(string name, long uncompressedSize, ushort headerSize = 30, IEnumerable<AppxBlock> blocks = null)
        {
            this.Name = name;
            this.UncompressedSize = uncompressedSize;
            this.HeaderSize = headerSize;
            this.Blocks = blocks == null ? new List<AppxBlock>() : new List<AppxBlock>(blocks);
        }

        public ushort HeaderSize { get; }

        public string Name { get; }

        public long UncompressedSize { get; }

        public IList<AppxBlock> Blocks { get; }

        public long UpdateImpact { get; set; }

        public long SizeDifference { get; set; }

        public ComparisonStatus Status { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}