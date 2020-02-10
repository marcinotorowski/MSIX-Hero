

// namespaces...

namespace External.EricZimmerman.Registry.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    // internal classes...
    public class LiListRecord : IListTemplate, IRecordBase
    {
        // private fields...

        private readonly int _size;

        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="LiListRecord" />  class.
        /// </summary>
        /// <param name="rawBytes"></param>
        /// <param name="relativeOffset"></param>
        public LiListRecord(byte[] rawBytes, long relativeOffset)
        {
            this.RelativeOffset = relativeOffset;
            this.RawBytes = rawBytes;
            this._size = BitConverter.ToInt32(rawBytes, 0);
        }

        /// <summary>
        ///     A list of relative offsets to other records
        /// </summary>
        public List<uint> Offsets
        {
            get
            {
                var offsets = new List<uint>();

                var index = 0x8;
                var counter = 0;

                while (counter < this.NumberOfEntries)
                {
                    var os = BitConverter.ToUInt32(this.RawBytes, index);
                    index += 4;

                    if (os == 0x0)
                    {
                        //ncrunch: no coverage
                        //there are cases where we run out of data before getting to NumberOfEntries. This stops an explosion
                        break; //ncrunch: no coverage
                    }

                    offsets.Add(os);

                    counter += 1;
                }

                return offsets;
            }
        }

        // public properties...

        public bool IsFree => this._size > 0;

        public bool IsReferenced { get; set; }

        public int NumberOfEntries => BitConverter.ToUInt16(this.RawBytes, 0x06);

        public byte[] RawBytes { get; }
        public long RelativeOffset { get; }

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

            var i = 0;

            foreach (var offset in this.Offsets)
            {
                sb.AppendLine($"------------ Offset #{i} ------------");
                sb.AppendLine($"Offset: 0x{offset:X}");
                i += 1;
            }

            sb.AppendLine();
            sb.AppendLine("------------ End of offsets ------------");
            sb.AppendLine();


            return sb.ToString();
        }
    }
}