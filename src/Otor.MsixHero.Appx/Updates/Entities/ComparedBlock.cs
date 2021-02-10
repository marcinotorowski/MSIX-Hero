namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparedBlock
    {
        public ComparedBlock(string hash, long compressedSize)
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