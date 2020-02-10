

// namespaces...

namespace External.EricZimmerman.Registry.Cells
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    // public classes...
    public class NkCellRecord : ICellTemplate, IRecordBase
    {
        [Flags]
        public enum AccessFlag
        {
            PreInitAccess = 0x01,
            PostInitAccess = 0x02
        }

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

        [Flags]
        public enum UserFlag
        {
            None = 0x0,
            Is32BitKey = 0x01,
            ReflectionCreated = 0x02,
            DisableReflection = 0x04,
            LegacyVista = 0x08
        }

        [Flags]
        public enum VirtualizationControlFlag
        {
            None = 0x0,
            DoNotVirtualize = 0x02,
            DoNotSilentFail = 0x04,
            Recursive = 0x08
        }

        private readonly int _rawBytesLength;
        private readonly IRegistry _registryHive;

        // public fields...
        public List<ulong> ValueOffsets;

        // protected internal constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="NkCellRecord" /> class.
        ///     <remarks>Represents a Key Node Record</remarks>
        /// </summary>
        protected internal NkCellRecord(int recordSize, long relativeOffset, IRegistry registryHive)
        {
            this.RelativeOffset = relativeOffset;
            this._registryHive = registryHive;
            this._rawBytesLength = recordSize;

            this.ValueOffsets = new List<ulong>();
        }

        /// <summary>
        ///     The relative offset to a data node containing the classname
        ///     <remarks>
        ///         Use ClassLength to get the correct classname vs using the entire contents of the data node. There is often
        ///         slack slace in the data node when they hold classnames
        ///     </remarks>
        /// </summary>
        public uint ClassCellIndex
        {
            get
            {
                var num = BitConverter.ToUInt32(this.RawBytes, 0x34);

                if (num == 0xFFFFFFFF)
                {
                    return 0;
                }

                return num;
            }
        }

        /// <summary>
        ///     The length of the classname in the data node referenced by ClassCellIndex.
        /// </summary>
        public ushort ClassLength => BitConverter.ToUInt16(this.RawBytes, 0x4e);

        /// <summary>
        ///     This is a Flags based enum, but is disabled in retail versions of Windows, so the flags are not broken down here.
        /// </summary>
        public byte Debug => this.RawBytes[0x3b];

        //TODO Layer semantics?
        public AccessFlag Access => (AccessFlag) BitConverter.ToInt32(this.RawBytes, 0x10);

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

        public uint MaximumClassLength => BitConverter.ToUInt32(this.RawBytes, 0x3c);

        public ushort MaximumNameLength => BitConverter.ToUInt16(this.RawBytes, 0x38);

        public uint MaximumValueDataLength => BitConverter.ToUInt32(this.RawBytes, 0x44);

        public uint MaximumValueNameLength => BitConverter.ToUInt32(this.RawBytes, 0x40);

        /// <summary>
        ///     The name of this key. This is what is shown on the left side of RegEdit in the key and subkey tree.
        /// </summary>
        public string Name
        {
            get
            {
                string name;

                if ((this.Flags & FlagEnum.CompressedName) == FlagEnum.CompressedName)
                {
                    if (this.IsFree)
                    {
                        if (this.RawBytes.Length >= 0x50 + this.NameLength)
                        {
                            name = Encoding.GetEncoding(1252).GetString(this.RawBytes, 0x50, this.NameLength);
                        }
                        else
                        {
                            name = "(Unable to determine name)";
                        }
                    }
                    else
                    {
                        name = Encoding.GetEncoding(1252).GetString(this.RawBytes, 0x50, this.NameLength);
                    }
                }
                else
                {
                    if (this.IsFree)
                    {
                        if (this.RawBytes.Length >= 0x50 + this.NameLength)
                        {
                            name = Encoding.Unicode.GetString(this.RawBytes, 0x50, this.NameLength);
                        }
                        else
                        {
                            name = "(Unable to determine name)";
                        }
                    }
                    else
                    {
                        name = Encoding.Unicode.GetString(this.RawBytes, 0x50, this.NameLength);
                    }
                }

                return name;
            }
        }

        public ushort NameLength => BitConverter.ToUInt16(this.RawBytes, 0x4c);

        public byte[] Padding
        {
            get
            {
                if (this.IsFree)
                {
                    return new byte[0];
                }

                var paddingOffset = 0x50 + this.NameLength;

                var paddingBlock = (int) Math.Ceiling((double) paddingOffset / 8);

                var actualPaddingOffset = paddingBlock * 8;

                var paddingLength = actualPaddingOffset - paddingOffset;

                if (paddingLength > 0 && paddingOffset + paddingLength <= this.RawBytes.Length)
                {
                    return new ArraySegment<byte>(this.RawBytes, paddingOffset, paddingLength).ToArray();
                }

                return new byte[0];
            }
        }

        /// <summary>
        ///     The relative offset to the parent key for this record
        /// </summary>
        public uint ParentCellIndex => BitConverter.ToUInt32(this.RawBytes, 0x14);

        /// <summary>
        ///     The relative offset to the security record for this record
        /// </summary>
        public uint SecurityCellIndex => BitConverter.ToUInt32(this.RawBytes, 0x30);

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
        ///     The relative offset to a list (or list of lists) that points to other NKRecords. These records are subkeys of this
        ///     key.
        /// </summary>
        public uint SubkeyListsStableCellIndex
        {
            get
            {
                var num = BitConverter.ToUInt32(this.RawBytes, 0x20);

                if (num == 0xFFFFFFFF)
                {
                    return 0;
                }

                return num;
            }
        }

        public uint SubkeyListsVolatileCellIndex
        {
            get
            {
                var num = BitConverter.ToUInt32(this.RawBytes, 0x24);

                if (num == 0xFFFFFFFF)
                {
                    num = 0;
                }

                return num;
            }
        }

        public UserFlag UserFlags
        {
            get
            {
                var rawFlags = Convert.ToString(this.RawBytes[0x3a], 2).PadLeft(8, '0');

                return (UserFlag) Convert.ToInt32(rawFlags.Substring(0, 4));
            }
        }

        /// <summary>
        ///     The relative offset to a list of VKrecords for this key
        /// </summary>
        public uint ValueListCellIndex
        {
            get
            {
                var num = BitConverter.ToUInt32(this.RawBytes, 0x2c);

                if (num == 0xFFFFFFFF)
                {
                    return 0;
                }

                return num;
            }
        }

        /// <summary>
        ///     The number of values this key contains
        /// </summary>
        public uint ValueListCount => BitConverter.ToUInt32(this.RawBytes, 0x28);

        public VirtualizationControlFlag VirtualControlFlags
        {
            get
            {
                var rawFlags = Convert.ToString(this.RawBytes[0x3a], 2).PadLeft(8, '0');

                return (VirtualizationControlFlag) Convert.ToInt32(rawFlags.Substring(4, 4));
            }
        }

        /// <summary>
        ///     Unused starting with Windows XP
        /// </summary>
        public uint WorkVar => BitConverter.ToUInt32(this.RawBytes, 0x48);

        // public properties...
        public long AbsoluteOffset => this.RelativeOffset + 4096;

        public bool IsFree => BitConverter.ToInt32(this.RawBytes, 0) > 0;

        public bool IsReferenced { get; internal set; }

        public byte[] RawBytes
        {
            get
            {
                var raw = this._registryHive.ReadBytesFromHive(this.AbsoluteOffset, this._rawBytesLength);
                return raw;
            }
        }

        public long RelativeOffset { get; }

        public string Signature => Encoding.GetEncoding(1252).GetString(this.RawBytes, 4, 2);

        public int Size => Math.Abs(BitConverter.ToInt32(this.RawBytes, 0));

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{Math.Abs(this.Size):X}");
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
            sb.AppendLine($"Subkey Lists Stable Cell Index: 0x{this.SubkeyListsStableCellIndex:X}");

            sb.AppendLine();
            sb.AppendLine($"Subkey Counts Volatile: 0x{this.SubkeyCountsVolatile:X}");

            sb.AppendLine();
            sb.AppendLine($"User Flags: 0x{this.UserFlags:X}");
            sb.AppendLine($"Virtual Control Flags: 0x{this.VirtualControlFlags:X}");
            sb.AppendLine($"Work Var: 0x{this.WorkVar:X}");

            sb.AppendLine();
            sb.AppendLine($"Value Count: 0x{this.ValueListCount:X}");
            sb.AppendLine($"Value List Cell Index: 0x{this.ValueListCellIndex:X}");

            if (this.Padding.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Padding: {BitConverter.ToString(this.Padding)}");
            }

            return sb.ToString();
        }
    }
}