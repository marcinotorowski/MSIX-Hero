namespace PriFormat
{
    public struct ByteSpan
    {
        public long Offset;
        public uint Length;

        internal ByteSpan(long offset, uint length)
        {
            Offset = offset;
            Length = length;
        }
    }
}
