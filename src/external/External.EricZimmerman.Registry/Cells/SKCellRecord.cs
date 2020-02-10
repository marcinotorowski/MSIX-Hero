

// namespaces...

namespace External.EricZimmerman.Registry.Cells
{
    using System;
    using System.Linq;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    // public classes...
    public class SkCellRecord : ICellTemplate, IRecordBase
    {
        // private fields...
        private readonly int _size;

        // protected internal constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="SkCellRecord" /> class.
        ///     <remarks>Represents a Key Security Record</remarks>
        /// </summary>
        protected internal SkCellRecord(byte[] rawBytes, long relativeOffset)
        {
            this.RelativeOffset = relativeOffset;
            this.RawBytes = rawBytes;

            this._size = BitConverter.ToInt32(rawBytes, 0);

            //this has to be a multiple of 8, so check for it
            var paddingOffset = 0x18 + this.DescriptorLength;
            var paddingLength = rawBytes.Length - paddingOffset;

            if (paddingLength > 0)
            {
                var padding = rawBytes.Skip((int) paddingOffset).Take((int) paddingLength).ToArray();

                //Check.That(Array.TrueForAll(padding, a => a == 0));
            }

            //Check that we have accounted for all bytes in this record. this ensures nothing is hidden in this record or there arent additional data structures we havent processed in the record.
            //   Check.That(0x18 + (int) DescriptorLength + paddingLength).IsEqualTo(rawBytes.Length);
        }

        // public properties...

        /// <summary>
        ///     A relative offset to the previous SK record
        /// </summary>
        public uint BLink => BitConverter.ToUInt32(this.RawBytes, 0x0c);

        public uint DescriptorLength => BitConverter.ToUInt32(this.RawBytes, 0x14);

        /// <summary>
        ///     A relative offset to the next SK record
        /// </summary>
        public uint FLink => BitConverter.ToUInt32(this.RawBytes, 0x08);

        /// <summary>
        ///     A count of how many keys this security record applies to
        /// </summary>
        public uint ReferenceCount => BitConverter.ToUInt32(this.RawBytes, 0x10);

        public ushort Reserved => BitConverter.ToUInt16(this.RawBytes, 0x6);

        /// <summary>
        ///     The security descriptor object for this record
        /// </summary>
        public SkSecurityDescriptor SecurityDescriptor
        {
            get
            {
                var rawDescriptor = this.RawBytes.Skip(0x18).Take((int) this.DescriptorLength).ToArray();

                if (rawDescriptor.Length > 0)
                {
                    // i have seen cases where there is no available security descriptor because the sk record doesn't contain the right data
                    return new SkSecurityDescriptor(rawDescriptor);
                }

                return null; //ncrunch: no coverage
            }
        }

        // public properties...
        public long AbsoluteOffset => this.RelativeOffset + 4096;

        public bool IsFree => this._size > 0;

        public bool IsReferenced { get; internal set; }
        public byte[] RawBytes { get; }
        public long RelativeOffset { get; }

        public string Signature => Encoding.ASCII.GetString(this.RawBytes, 4, 2);

        public int Size => Math.Abs(this._size);

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{Math.Abs(this._size):X}");
            sb.AppendLine($"Relative Offset: 0x{this.RelativeOffset:X}");
            sb.AppendLine($"Absolute Offset: 0x{this.AbsoluteOffset:X}");
            sb.AppendLine($"Signature: {this.Signature}");

            sb.AppendLine($"Is Free: {this.IsFree}");

            sb.AppendLine();
            sb.AppendLine($"Forward Link: 0x{this.FLink:X}");
            sb.AppendLine($"Backward Link: 0x{this.BLink:X}");
            sb.AppendLine();

            sb.AppendLine($"Reference Count: {this.ReferenceCount:N0}");

            sb.AppendLine();
            sb.AppendLine($"Security descriptor length: 0x{this.DescriptorLength:X}");

            sb.AppendLine();
            sb.AppendLine($"Security descriptor: {this.SecurityDescriptor}");

            return sb.ToString();
        }
    }
}