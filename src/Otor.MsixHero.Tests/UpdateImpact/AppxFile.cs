using System;
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Tests.UpdateImpact
{
    public class AppxFile : IEquatable<AppxFile>
    {
        public AppxFile(string name, long size, IEnumerable<AppxBlock> blocks = null)
        {
            this.Name = name;
            this.Size = size;
            this.Blocks = blocks == null ? new List<AppxBlock>() : new List<AppxBlock>(blocks);
        }

        public string Name { get; set; }

        public long Size { get; set; }

        public IList<AppxBlock> Blocks { get; set; }

        public bool Equals(AppxFile other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (this.Size != other.Size || this.Name != other.Name)
            {
                return false;
            }

            return this.Blocks.SequenceEqual(other.Blocks);
        }
    }
}