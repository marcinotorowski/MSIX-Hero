

// namespaces...

namespace External.EricZimmerman.Registry.Other
{
    using System;
    using System.Text;

    [Flags]
    public enum KtmFlag
    {
        Unset = 0x0,
        KtmLocked = 0x1,
        Defragmented = 0x2
    }

    // public classes...
    public class RegistryHeader
    {
        public int CalculatedChecksum;

        // protected internal constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="RegistryHeader" /> class.
        /// </summary>
        protected internal RegistryHeader(byte[] rawBytes)
        {
            this.FileName = string.Empty;
            this.Signature = Encoding.ASCII.GetString(rawBytes, 0, 4);


            if (this.Signature != "regf")
            {
                throw new Exception("This is not a Registry hive. Header != 'regf'");
            }

            this.PrimarySequenceNumber = BitConverter.ToUInt32(rawBytes, 0x4);
            this.SecondarySequenceNumber = BitConverter.ToUInt32(rawBytes, 0x8);

            var ts = BitConverter.ToInt64(rawBytes, 0xc);

            this.LastWriteTimestamp = DateTimeOffset.FromFileTime(ts).ToUniversalTime();

            this.MajorVersion = BitConverter.ToInt32(rawBytes, 0x14);
            this.MinorVersion = BitConverter.ToInt32(rawBytes, 0x18);

            this.Type = BitConverter.ToUInt32(rawBytes, 0x1c);

            this.Format = BitConverter.ToUInt32(rawBytes, 0x20);

            this.RootCellOffset = BitConverter.ToUInt32(rawBytes, 0x24);

            this.Length = BitConverter.ToUInt32(rawBytes, 0x28);

            this.Cluster = BitConverter.ToUInt32(rawBytes, 0x2c);

            this.FileName = Encoding.Unicode.GetString(rawBytes, 0x30, 64)
                .Replace("\0", string.Empty)
                .Replace("\\??\\", string.Empty);


            //in windows 10, some extra things are added in reserved area, starting at offset 0x70

            var gbuff = new byte[16];
            Buffer.BlockCopy(rawBytes, 0x70, gbuff, 0, 16);

            this.ResourceManagerGuid = new Guid(gbuff);

            gbuff = new byte[16];
            Buffer.BlockCopy(rawBytes, 0x80, gbuff, 0, 16);

            this.LogFilenameGuid = new Guid(gbuff);

            this.KtmFlags = (KtmFlag) BitConverter.ToInt32(rawBytes, 0x90);

            gbuff = new byte[16];
            Buffer.BlockCopy(rawBytes, 0x94, gbuff, 0, 16);

            this.TransactionManagerGuid = new Guid(gbuff);

            ts = BitConverter.ToInt64(rawBytes, 0xa8);

            try
            {
                this.LastReorganizedTimestamp = DateTimeOffset.FromFileTime(ts).ToUniversalTime();
            }
            catch (Exception)
            {
            }


            //End new

            this.CheckSum = BitConverter.ToInt32(rawBytes, 0x1fc);

            var index = 0;
            var xsum = 0;
            while (index <= 0x1fb)
            {
                xsum ^= BitConverter.ToInt32(rawBytes, index);
                index += 0x04;
            }

            this.CalculatedChecksum = xsum;

            this.BootType = BitConverter.ToUInt32(rawBytes, 0xff8);
            this.BootRecover = BitConverter.ToUInt32(rawBytes, 0xffc);
        }

        public Guid ResourceManagerGuid { get; }
        public Guid TransactionManagerGuid { get; }
        public Guid LogFilenameGuid { get; }

        public KtmFlag KtmFlags { get; }

        // public properties...
        public uint BootRecover { get; }

        public uint BootType { get; }
        public int CheckSum { get; }
        public uint Cluster { get; }

        /// <summary>
        ///     Registry hive's embedded filename
        /// </summary>
        public string FileName { get; }

        public uint Format { get; }

        /// <summary>
        ///     The last write timestamp of the registry hive
        /// </summary>
        public DateTimeOffset LastWriteTimestamp { get; }

        /// <summary>
        ///     The last write timestamp of the registry hive
        /// </summary>
        public DateTimeOffset? LastReorganizedTimestamp { get; }

        /// <summary>
        ///     The total number of bytes used by this hive
        /// </summary>
        public uint Length { get; }

        public int MajorVersion { get; }
        public int MinorVersion { get; }

        /// <summary>
        ///     The offset in the first hbin record where root key is found
        /// </summary>
        public uint RootCellOffset { get; }

        public uint PrimarySequenceNumber { get; }
        public uint SecondarySequenceNumber { get; }

        /// <summary>
        ///     Signature of the registry hive. Should always be "regf"
        /// </summary>
        public string Signature { get; }

        public uint Type { get; }

        public bool ValidateCheckSum(bool ignore = false)
        {
            return this.CheckSum == this.CalculatedChecksum;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Signature: {this.Signature}");

            sb.AppendLine($"FileName: {this.FileName}");

            sb.AppendLine();

            sb.AppendLine($"PrimarySequenceNumber: 0x{this.PrimarySequenceNumber:X}");
            sb.AppendLine($"SecondarySequenceNumber: 0x{this.SecondarySequenceNumber:X}");

            sb.AppendLine();

            sb.AppendLine($"Last Write Timestamp: {this.LastWriteTimestamp}");

            sb.AppendLine();

            sb.AppendLine($"Major version: {this.MajorVersion}");
            sb.AppendLine($"Minor version: {this.MinorVersion}");

            sb.AppendLine();
            sb.AppendLine($"Type: 0x{this.Type:X}");
            sb.AppendLine($"Format: 0x{this.Format:X}");

            sb.AppendLine();
            sb.AppendLine($"RootCellOffset: 0x{this.RootCellOffset:X}");

            sb.AppendLine();
            sb.AppendLine($"Length: 0x{this.Length:X}");

            sb.AppendLine();
            sb.AppendLine($"Cluster: 0x{this.Cluster:X}");

            sb.AppendLine();
            sb.AppendLine($"CheckSum: 0x{this.CheckSum:X}");
            sb.AppendLine($"CheckSum: 0x{this.CalculatedChecksum:X}");
            sb.AppendLine($"CheckSums match: {this.CalculatedChecksum == this.CheckSum}");

            sb.AppendLine();
            sb.AppendLine($"BootType: 0x{this.BootType:X}");
            sb.AppendLine($"BootRecover: 0x{this.BootRecover:X}");

            return sb.ToString();
        }
    }
}