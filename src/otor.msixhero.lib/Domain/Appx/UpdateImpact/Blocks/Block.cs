namespace otor.msixhero.lib.Domain.Appx.UpdateImpact.Blocks
{
    public class Block
    {
        public string FileName { get; set; }

        public string Hash { get; set; }

        public long Length { get; set; }

        public long Position { get; set; }

        public BlockType BlockType { get; set; }
    }
}