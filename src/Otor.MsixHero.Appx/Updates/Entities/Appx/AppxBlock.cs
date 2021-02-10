using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    public class AppxBlock
    {
        public AppxBlock(string hash, long compressedSize)
        {
            this.Hash = hash;
            this.CompressedSize = compressedSize;
        }

        public string Hash { get; }

        public long CompressedSize { get; }

        public ComparisonStatus Status { get; set; }

        public long UpdateImpact { get; set; }

        public override string ToString()
        {
            return this.Hash;
        }
    }
}