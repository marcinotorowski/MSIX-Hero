

// namespaces...

namespace External.EricZimmerman.Registry.Other
{
    using System;
    using System.Text;

    // internal classes...
    // public classes...
    public class DataNode : IRecordBase
    {
        // private fields...
        private readonly int _size;

        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataNode" /> class.
        /// </summary>
        public DataNode(byte[] rawBytes, long relativeOffset)
        {
            this.RelativeOffset = relativeOffset;

            this.RawBytes = rawBytes;

            this._size = BitConverter.ToInt32(rawBytes, 0);
        }

        // public properties...
        public byte[] Data => new ArraySegment<byte>(this.RawBytes, 4, this.RawBytes.Length - 4).ToArray();

        public bool IsFree => this._size > 0;

        public byte[] RawBytes { get; }

        /// <summary>
        ///     Set to true when a record is referenced by another referenced record.
        ///     <remarks>
        ///         This flag allows for determining records that are marked 'in use' by their size but never actually
        ///         referenced by another record in a hive
        ///     </remarks>
        /// </summary>
        public bool IsReferenced { get; internal set; }

        /// <summary>
        ///     The offset as stored in other records to a given record
        ///     <remarks>This value will be 4096 bytes (the size of the regf header) less than the AbsoluteOffset</remarks>
        /// </summary>
        public long RelativeOffset { get; }

        public int Size => Math.Abs(this._size);

        // public properties...
        /// <summary>
        ///     The offset to this record from the beginning of the hive, in bytes
        /// </summary>
        public long AbsoluteOffset => this.RelativeOffset + 4096;

        public string Signature => string.Empty;

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{this.Size:X}");
            sb.AppendLine($"Relative Offset: 0x{this.RelativeOffset:X}");
            sb.AppendLine($"Absolute Offset: 0x{this.AbsoluteOffset:X}");

            sb.AppendLine();

            sb.AppendLine($"Is Free: {this.IsFree}");

            sb.AppendLine();

            sb.AppendLine($"Raw Bytes: {BitConverter.ToString(this.RawBytes)}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}