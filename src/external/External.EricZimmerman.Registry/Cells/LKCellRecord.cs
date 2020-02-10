

// namespaces...


//ncrunch: no coverage start

namespace External.EricZimmerman.Registry.Cells
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    // public classes...
    public class LkCellRecord : ICellTemplate, IRecordBase
    {
        // public enums...
        [Flags]
        public enum FlagEnum
        {
            CompressedName = 0x0020,
            HiveEntryRootKey = 0x0004,
            HiveExit = 0x0002,
            NoDelete = 0x0008,
            PredefinedHandle = 0x0040,
            SymbolicLink = 0x0010,
            Unused0400 = 0x0400,
            Unused0800 = 0x0800,
            Unused1000 = 0x1000,
            Unused2000 = 0x2000,
            Unused4000 = 0x4000,
            Unused8000 = 0x8000,
            UnusedVolatileKey = 0x0001,
            VirtMirrored = 0x0080,
            VirtTarget = 0x0100,
            VirtualStore = 0x0200
        }

        // private fields...
        private readonly int _size;
        // protected internal constructors...

        // public fields...
        public List<ulong> ValueOffsets;

        // protected internal constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="NkCellRecord" /> class.
        ///     <remarks>Represents a Key Node Record</remarks>
        /// </summary>
        protected internal LkCellRecord(byte[] rawBytes, long relativeOffset)
        {
            this.RelativeOffset = relativeOffset;
            this.RawBytes = rawBytes;

            this.ValueOffsets = new List<ulong>();

            this._size = BitConverter.ToInt32(rawBytes, 0);

            //TODO FINISH THIS LIKE NK

            //RootCellIndex
            var num = BitConverter.ToUInt32(rawBytes, 0x20);

            if (num == 0xFFFFFFFF)
            {
                this.RootCellIndex = 0;
            }
            else
            {
                this.RootCellIndex = num;
            }

            //HivePointer
            num = BitConverter.ToUInt32(rawBytes, 0x24);

            if (num == 0xFFFFFFFF)
            {
                this.HivePointer = 0;
            }
            else
            {
                this.HivePointer = num;
            }

            this.SecurityCellIndex = BitConverter.ToUInt32(rawBytes, 0x30);

            //ClassCellIndex
            num = BitConverter.ToUInt32(rawBytes, 0x34);

            if (num == 0xFFFFFFFF)
            {
                this.ClassCellIndex = 0;
            }
            else
            {
                this.ClassCellIndex = num;
            }

            this.MaximumNameLength = BitConverter.ToUInt16(rawBytes, 0x38);

            var rawFlags = Convert.ToString(rawBytes[0x3a], 2).PadLeft(8, '0');

            //TODO is this a flag enum somewhere?
            var userInt = Convert.ToInt32(rawFlags.Substring(0, 4));

            var virtInt = Convert.ToInt32(rawFlags.Substring(4, 4));

            this.UserFlags = userInt;
            this.VirtualControlFlags = virtInt;

            this.Debug = rawBytes[0x3b];

            this.MaximumClassLength = BitConverter.ToUInt32(rawBytes, 0x3c);
            this.MaximumValueNameLength = BitConverter.ToUInt32(rawBytes, 0x40);
            this.MaximumValueDataLength = BitConverter.ToUInt32(rawBytes, 0x44);

            this.WorkVar = BitConverter.ToUInt32(rawBytes, 0x48);

            this.NameLength = BitConverter.ToUInt16(rawBytes, 0x4c);
            this.ClassLength = BitConverter.ToUInt16(rawBytes, 0x4e);

            //  if (Flags.ToString().Contains(FlagEnum.CompressedName.ToString()))
            if ((this.Flags & FlagEnum.CompressedName) == FlagEnum.CompressedName)
            {
                this.Name = Encoding.GetEncoding(1252).GetString(rawBytes, 0x50, this.NameLength);
            }
            else
            {
                this.Name = Encoding.Unicode.GetString(rawBytes, 0x50, this.NameLength);
            }

            var paddingOffset = 0x50 + this.NameLength;
            var paddingLength = Math.Abs(this.Size) - paddingOffset;

            if (paddingLength > 0)
            {
                this.Padding = new byte[paddingLength];
                Array.Copy(rawBytes, paddingOffset, this.Padding, 0, paddingLength);
                //Padding = BitConverter.ToString(rawBytes, paddingOffset, paddingLength);
            }

            //we have accounted for all bytes in this record. this ensures nothing is hidden in this record or there arent additional data structures we havent processed in the record.
            
        }

        // public properties...
        /// <summary>
        ///     The relative offset to a data node containing the classname
        ///     <remarks>
        ///         Use ClassLength to get the correct classname vs using the entire contents of the data node. There is often
        ///         slack slace in the data node when they hold classnames
        ///     </remarks>
        /// </summary>
        public uint ClassCellIndex { get; }

        /// <summary>
        ///     The length of the classname in the data node referenced by ClassCellIndex.
        /// </summary>
        public ushort ClassLength { get; }

        public byte Debug { get; }

        public FlagEnum Flags => (FlagEnum) BitConverter.ToUInt16(this.RawBytes, 6);

        /// <summary>
        ///     The last write time of this key
        /// </summary>
        public DateTimeOffset LastWriteTimestamp
        {
            get
            {
                var ts = BitConverter.ToInt64(this.RawBytes, 0x8);

                return DateTimeOffset.FromFileTime(ts).ToUniversalTime();
            }
        }

        public uint MaximumClassLength { get; }
        public ushort MaximumNameLength { get; }
        public uint MaximumValueDataLength { get; }
        public uint MaximumValueNameLength { get; }

        /// <summary>
        ///     The name of this key. This is what is shown on the left side of RegEdit in the key and subkey tree.
        /// </summary>
        public string Name { get; }

        public ushort NameLength { get; }
        public byte[] Padding { get; }

        /// <summary>
        ///     The relative offset to the parent key for this record
        /// </summary>
        public uint ParentCellIndex => BitConverter.ToUInt32(this.RawBytes, 0x14);

        /// <summary>
        ///     The relative offset to the security record for this record
        /// </summary>
        public uint SecurityCellIndex { get; }

        /// <summary>
        ///     When true, this key has been deleted
        ///     <remarks>
        ///         The parent key is determined by checking whether ParentCellIndex 1) exists and 2)
        ///         ParentCellIndex.IsReferenced == true.
        ///     </remarks>
        /// </summary>
        public bool IsDeleted { get; internal set; }

        /// <summary>
        ///     The number of subkeys this key contains
        /// </summary>
        public uint SubkeyCountsStable => BitConverter.ToUInt32(this.RawBytes, 0x18);

        public uint SubkeyCountsVolatile => BitConverter.ToUInt32(this.RawBytes, 0x1c);

        /// <summary>
        ///     The relative offset to the root cell this record is linked to.
        /// </summary>
        public uint RootCellIndex { get; }

        public uint HivePointer { get; }
        public int UserFlags { get; }
        public int VirtualControlFlags { get; }

        public uint WorkVar { get; }

        // public properties...
        public long AbsoluteOffset
        {
            get => this.RelativeOffset + 4096;
            set { }
        }

        public bool IsFree => this._size > 0;

        public bool IsReferenced { get; internal set; }
        public byte[] RawBytes { get; }
        public long RelativeOffset { get; }

        public string Signature
        {
            get => Encoding.ASCII.GetString(this.RawBytes, 4, 2);
            set { }
        }

        public int Size => Math.Abs(this._size);

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{Math.Abs(this._size):X}");
            sb.AppendLine($"Relative Offset: 0x{this.RelativeOffset:X}");
            sb.AppendLine($"Absolute Offset: 0x{this.AbsoluteOffset:X}");
            sb.AppendLine($"Signature: {this.Signature}");
            sb.AppendLine($"Flags: {this.Flags}");
            sb.AppendLine();
            sb.AppendLine($"Name: {this.Name}");
            sb.AppendLine();
            sb.AppendLine($"Last Write Timestamp: {this.LastWriteTimestamp}");
            sb.AppendLine();

            sb.AppendLine($"Is Free: {this.IsFree}");

            sb.AppendLine();
            sb.AppendLine($"Debug: 0x{this.Debug:X}");

            sb.AppendLine();
            sb.AppendLine($"Maximum Class Length: 0x{this.MaximumClassLength:X}");
            sb.AppendLine($"Class Cell Index: 0x{this.ClassCellIndex:X}");
            sb.AppendLine($"Class Length: 0x{this.ClassLength:X}");

            sb.AppendLine();

            sb.AppendLine($"Maximum Value Data Length: 0x{this.MaximumValueDataLength:X}");
            sb.AppendLine($"Maximum Value Name Length: 0x{this.MaximumValueNameLength:X}");

            sb.AppendLine();
            sb.AppendLine($"Name Length: 0x{this.NameLength:X}");
            sb.AppendLine($"Maximum Name Length: 0x{this.MaximumNameLength:X}");

            sb.AppendLine();
            sb.AppendLine($"Parent Cell Index: 0x{this.ParentCellIndex:X}");
            sb.AppendLine($"Security Cell Index: 0x{this.SecurityCellIndex:X}");

            sb.AppendLine();
            sb.AppendLine($"Subkey Counts Stable: 0x{this.SubkeyCountsStable:X}");

            sb.AppendLine();
            sb.AppendLine($"Hive pointer: 0x{this.HivePointer:X}");
            sb.AppendLine($"Root cell index: 0x{this.RootCellIndex:X}");

            sb.AppendLine();
            sb.AppendLine($"Subkey Counts Volatile: 0x{this.SubkeyCountsVolatile:X}");

            sb.AppendLine();
            sb.AppendLine($"User Flags: 0x{this.UserFlags:X}");
            sb.AppendLine($"Virtual Control Flags: 0x{this.VirtualControlFlags:X}");
            sb.AppendLine($"Work Var: 0x{this.WorkVar:X}");


            sb.AppendLine();
            sb.AppendLine($"Padding: {BitConverter.ToString(this.Padding)}");

            return sb.ToString();
        }
    }
}