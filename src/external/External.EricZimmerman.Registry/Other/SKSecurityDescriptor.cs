

// namespaces...

namespace External.EricZimmerman.Registry.Other
{
    using System;
    using System.Linq;
    using System.Text;

    // public classes...
    public class SkSecurityDescriptor
    {
        // public enums...
        [Flags]
        public enum ControlEnum
        {
            SeDaclAutoInherited = 0x0400,
            SeDaclAutoInheritReq = 0x0100,
            SeDaclDefaulted = 0x0008,
            SeDaclPresent = 0x0004,
            SeDaclProtected = 0x1000,
            SeGroupDefaulted = 0x0002,
            SeOwnerDefaulted = 0x0001,
            SeRmControlValid = 0x4000,
            SeSaclAutoInherited = 0x0800,
            SeSaclAutoInheritReq = 0x0200,
            SeSaclDefaulted = 0x0020,
            SeSaclPresent = 0x0010,
            SeSaclProtected = 0x2000,
            SeSelfRelative = 0x8000
        }

        private readonly uint _sizeDacl;
        private readonly uint _sizeGroupSid;
        private readonly uint _sizeOwnerSid;

        private readonly uint _sizeSacl;

        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="SkSecurityDescriptor" /> class.
        /// </summary>
        public SkSecurityDescriptor(byte[] rawBytes)
        {
            this.RawBytes = rawBytes;

            this._sizeSacl = this.DaclOffset - this.SaclOffset;
            this._sizeDacl = this.OwnerOffset - this.DaclOffset;
            this._sizeOwnerSid = this.GroupOffset - this.OwnerOffset;
            this._sizeGroupSid = (uint) (rawBytes.Length - this.GroupOffset);


            this.Padding = string.Empty; //TODO VERIFY ITS ALWAYS ZEROs
        }

        // public properties...
        public ControlEnum Control => (ControlEnum) BitConverter.ToUInt16(this.RawBytes, 0x02);

        public XAclRecord Dacl
        {
            get
            {
                if ((this.Control & ControlEnum.SeDaclPresent) == ControlEnum.SeDaclPresent)
                {
                    var rawDacl = this.RawBytes.Skip((int) this.DaclOffset).Take((int) this._sizeDacl).ToArray();
                    return new XAclRecord(rawDacl, XAclRecord.AclTypeEnum.Discretionary);
                }

                return null; //ncrunch: no coverage
            }
        }

        public uint DaclOffset => BitConverter.ToUInt32(this.RawBytes, 0x10);

        public uint GroupOffset => BitConverter.ToUInt32(this.RawBytes, 0x08);

        public string GroupSid
        {
            get
            {
                var rawGroup = this.RawBytes.Skip((int) this.GroupOffset).Take((int) this._sizeGroupSid).ToArray();
                return Helpers.ConvertHexStringToSidString(rawGroup);
            }
        }

        public Helpers.SidTypeEnum GroupSidType => Helpers.GetSidTypeFromSidString(this.GroupSid);

        public uint OwnerOffset => BitConverter.ToUInt32(this.RawBytes, 0x04);

        public string OwnerSid
        {
            get
            {
                var rawOwner = this.RawBytes.Skip((int) this.OwnerOffset).Take((int) this._sizeOwnerSid).ToArray();
                return Helpers.ConvertHexStringToSidString(rawOwner);
            }
        }

        public Helpers.SidTypeEnum OwnerSidType => Helpers.GetSidTypeFromSidString(this.OwnerSid);

        public string Padding { get; }
        public byte[] RawBytes { get; }

        public byte Revision => this.RawBytes[0];

        public XAclRecord Sacl
        {
            get
            {
                if ((this.Control & ControlEnum.SeSaclPresent) == ControlEnum.SeSaclPresent)
                {
                    var rawSacl = this.RawBytes.Skip((int) this.SaclOffset).Take((int) this._sizeSacl).ToArray();
                    return new XAclRecord(rawSacl, XAclRecord.AclTypeEnum.Security);
                }

                return null;
            }
        }

        public uint SaclOffset => BitConverter.ToUInt32(this.RawBytes, 0x0c);

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Revision: 0x{this.Revision:X}");
            sb.AppendLine($"Control: {this.Control}");

            sb.AppendLine();
            sb.AppendLine($"Owner offset: 0x{this.OwnerOffset:X}");
            sb.AppendLine($"Owner SID: {this.OwnerSid}");
            sb.AppendLine($"Owner SID Type: {this.OwnerSidType}");

            sb.AppendLine();
            sb.AppendLine($"Group offset: 0x{this.GroupOffset:X}");
            sb.AppendLine($"Group SID: {this.GroupSid}");
            sb.AppendLine($"Group SID Type: {this.GroupSidType}");

            if (this.Dacl != null)
            {
                sb.AppendLine();
                sb.AppendLine($"Dacl Offset: 0x{this.DaclOffset:X}");
                sb.AppendLine($"DACL: {this.Dacl}");
            }

            if (this.Sacl != null)
            {
                sb.AppendLine();
                sb.AppendLine($"Sacl Offset: 0x{this.SaclOffset:X}");
                sb.AppendLine($"SACL: {this.Sacl}");
            }

            return sb.ToString();
        }
    }
}