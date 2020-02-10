

// namespaces...

namespace External.EricZimmerman.Registry.Lists
{
    using System;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    // internal classes...
    public class DbListRecord : IListTemplate, IRecordBase
    {
        // private fields...
        private readonly int _size;

        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbListRecord" />  class.
        /// </summary>
        /// <param name="rawBytes"></param>
        /// <param name="relativeOffset"></param>
        public DbListRecord(byte[] rawBytes, long relativeOffset)
        {
            this.RelativeOffset = relativeOffset;
            this.RawBytes = rawBytes;
            if (rawBytes.Length == 0)
            {
               
                this._size = 0;
                return;
            }
            this._size = BitConverter.ToInt32(rawBytes, 0);
        }

        /// <summary>
        ///     The relative offset to another data node that contains a list of relative offsets to data for a VK record
        /// </summary>
        public uint OffsetToOffsets => BitConverter.ToUInt32(this.RawBytes, 0x8);

        // public properties...

        public bool IsFree => this._size > 0;

        public bool IsReferenced { get; set; }

        public int NumberOfEntries => BitConverter.ToUInt16(this.RawBytes, 0x06);

        public byte[] RawBytes { get; set; }
        public long RelativeOffset { get; set; }

        public string Signature => Encoding.ASCII.GetString(this.RawBytes, 4, 2);

        public int Size => Math.Abs(this._size);

        // public properties...
        public long AbsoluteOffset => this.RelativeOffset + 4096;

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{this.Size:X}");
            sb.AppendLine($"Relative Offset: 0x{this.RelativeOffset:X}");
            sb.AppendLine($"Absolute Offset: 0x{this.AbsoluteOffset:X}");

            sb.AppendLine($"Signature: {this.Signature}");

            sb.AppendLine();

            sb.AppendLine($"Is Free: {this.IsFree}");

            sb.AppendLine();

            sb.AppendLine($"Number Of Entries: {this.NumberOfEntries:N0}");
            sb.AppendLine();

            sb.AppendLine($"Offset To Offsets: 0x{this.OffsetToOffsets:X}");

            sb.AppendLine();

            return sb.ToString();
        }
    }
}