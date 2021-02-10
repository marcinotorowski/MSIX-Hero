using System;

namespace Otor.MsixHero.Tests.UpdateImpact
{
    public class AppxBlock : IEquatable<AppxBlock>
    {
        public AppxBlock(string hash, long size)
        {
            this.Hash = hash;
            this.Size = size;
        }

        public string Hash { get; }

        public long Size { get; }

        public bool Equals(AppxBlock other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return this.Hash == other.Hash;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as AppxBlock);
        }

        public override int GetHashCode()
        {
            return this.Hash.GetHashCode(StringComparison.Ordinal);
        }

        public static bool operator ==(AppxBlock block1, AppxBlock block2)
        {
            if (ReferenceEquals(block1, block2))
            {
                return true;
            }

            if (ReferenceEquals(block1, null) || ReferenceEquals(block2, null))
            {
                return false;
            }

            return block1.Hash == block2.Hash;
        }

        public static bool operator !=(AppxBlock block1, AppxBlock block2)
        {
            if (ReferenceEquals(block1, block2))
            {
                return false;
            }

            if (ReferenceEquals(block1, null) || ReferenceEquals(block2, null))
            {
                return true;
            }

            return block1.Hash != block2.Hash;
        }
    }
}