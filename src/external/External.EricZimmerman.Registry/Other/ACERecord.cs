

// namespaces...

namespace External.EricZimmerman.Registry.Other
{
    using System;
    using System.Linq;
    using System.Text;

    // public classes...
    public class AceRecord
    {
        // public enums...
        [Flags]
        public enum AceFlagsEnum
        {
            ContainerInheritAce = 0x02,
            FailedAccessAceFlag = 0x80,
            InheritedAce = 0x10,
            InheritOnlyAce = 0x08,
            None = 0x0,
            NoPropagateInheritAce = 0x04,
            ObjectInheritAce = 0x01,
            SuccessfulAccessAceFlag = 0x40
        }

        public enum AceTypeEnum
        {
            AccessAllowedAceType = 0x0,
            AccessAllowedCompoundAceType = 0x4,
            AccessAllowedObjectAceType = 0x5,
            AccessDeniedAceType = 0x1,
            AccessDeniedObjectAceType = 0x6,
            SystemAlarmAceType = 0x3,
            SystemAlarmObjectAceType = 0x8,
            SystemAuditAceType = 0x2,
            SystemAuditObjectAceType = 0x7,
            Unknown = 0x99
        }

        [Flags]
        public enum MasksEnum
        {
            CreateLink = 0x00000020,
            CreateSubkey = 0x00000004,
            Delete = 0x00010000,
            EnumerateSubkeys = 0x00000008,
            FullControl = 0x000F003F,
            Notify = 0x00000010,
            QueryValue = 0x00000001,
            ReadControl = 0x00020000,
            SetValue = 0x00000002,
            WriteDac = 0x00040000,
            WriteOwner = 0x00080000
        }

        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="AceRecord" /> class.
        /// </summary>
        public AceRecord(byte[] rawBytes)
        {
            this.RawBytes = rawBytes;
        }

        // public properties...
        public AceFlagsEnum AceFlags => (AceFlagsEnum) this.RawBytes[1];

        public ushort AceSize => BitConverter.ToUInt16(this.RawBytes, 2);

        public AceTypeEnum AceType
        {
            get
            {
                switch (this.RawBytes[0])
                {
                    case 0x0:
                        return AceTypeEnum.AccessAllowedAceType;
                    //ncrunch: no coverage start
                    case 0x1:
                        return AceTypeEnum.AccessDeniedAceType;

                    case 0x2:
                        return AceTypeEnum.SystemAuditAceType;

                    case 0x3:
                        return AceTypeEnum.SystemAlarmAceType;

                    case 0x4:
                        return AceTypeEnum.AccessAllowedCompoundAceType;

                    case 0x5:
                        return AceTypeEnum.AccessAllowedObjectAceType;

                    case 0x6:
                        return AceTypeEnum.AccessDeniedObjectAceType;

                    case 0x7:
                        return AceTypeEnum.SystemAuditObjectAceType;

                    case 0x8:
                        return AceTypeEnum.SystemAlarmObjectAceType;
                    default:
                        return AceTypeEnum.Unknown;
                    //ncrunch: no coverage end
                }
            }
        }

        public MasksEnum Mask => (MasksEnum) BitConverter.ToUInt32(this.RawBytes, 4);

        public byte[] RawBytes { get; }

        public string Sid
        {
            get
            {
                var rawSid = this.RawBytes.Skip(0x8).Take(this.AceSize - 0x8).ToArray();

                return Helpers.ConvertHexStringToSidString(rawSid);
            }
        }

        public Helpers.SidTypeEnum SidType => Helpers.GetSidTypeFromSidString(this.Sid);

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"ACE Size: 0x{this.AceSize:X}");

            sb.AppendLine($"ACE Type: {this.AceType}");

            sb.AppendLine($"ACE Flags: {this.AceFlags}");

            sb.AppendLine($"Mask: {this.Mask}");

            sb.AppendLine($"SID: {this.Sid}");
            sb.AppendLine($"SID Type: {this.SidType}");

            sb.AppendLine($"SID Type Description: {Helpers.GetDescriptionFromEnumValue(this.SidType)}");

            return sb.ToString();
        }
    }
}