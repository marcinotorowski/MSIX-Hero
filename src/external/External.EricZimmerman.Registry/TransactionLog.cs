namespace External.EricZimmerman.Registry
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    using NLog;

    public class TransactionLog
    {
        private const int RegfSignature = 0x66676572;
        internal readonly Logger Logger;

        private bool _parsed;

        public TransactionLog(byte[] rawBytes, string logFile)
        {
            this.FileBytes = rawBytes;
            this.LogPath = "None";

            this.Logger = LogManager.GetLogger("rawBytes");

            if (!this.HasValidSignature())
            {
                this.Logger.Error("Data in byte array is not a Registry transaction log (bad signature)");

                throw new ArgumentException("Data in byte array is not a Registry transaction log (bad signature)");
            }

            this.LogPath = logFile;

            this.TransactionLogEntries = new List<TransactionLogEntry>();

            this.Initialize();
        }

        public TransactionLog(string logFile)
        {
            if (logFile == null)
            {
                throw new ArgumentNullException(nameof(logFile));
            }

            if (!File.Exists(logFile))
            {
                throw new FileNotFoundException();
            }

            var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            var binaryReader = new BinaryReader(fileStream);

            binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            this.FileBytes = binaryReader.ReadBytes((int) binaryReader.BaseStream.Length);

            binaryReader.Close();
            fileStream.Close();

            if (this.FileBytes.Length == 0)
            {
                throw new Exception("0 byte log file. Nothing to do");
            }

            this.Logger = LogManager.GetLogger(logFile);

            if (!this.HasValidSignature())
            {
                this.Logger.Error($"'{logFile}' is not a Registry transaction log (bad signature)");

                throw new Exception($"'{logFile}' is not a Registry transaction log (bad signature)");
            }

            this.LogPath = logFile;

            this.TransactionLogEntries = new List<TransactionLogEntry>();

            this.Initialize();
        }

        public byte[] FileBytes { get; }

        public string LogPath { get; }
        public string Version { get; private set; }

        public RegistryHeader Header { get; set; }
        public HiveTypeEnum HiveType { get; private set; }
        public List<TransactionLogEntry> TransactionLogEntries { get; }

        public int NewSequenceNumber { get; private set; }

        private byte[] ReadBytesFromHive(long offset, int length)
        {
            var readLength = Math.Abs(length);

            var remaining = this.FileBytes.Length - offset;

            if (remaining <= 0)
            {
                return new byte[0];
            }

            if (readLength > remaining)
            {
                readLength = (int) remaining;
            }

            var r = new ArraySegment<byte>(this.FileBytes, (int) offset, readLength);

            return r.ToArray();
        }

        private void Initialize()
        {
            var header = this.ReadBytesFromHive(0, 4096);

         //   Logger.Trace("Getting header");

            this.Header = new RegistryHeader(header);

        //    Logger.Trace("Got header. Embedded file name {0}", Header.FileName);

            var fNameBase = Path.GetFileName(this.Header.FileName).ToLowerInvariant();

            switch (fNameBase)
            {
                case "ntuser.dat":
                    this.HiveType = HiveTypeEnum.NtUser;
                    break;
                case "sam":
                    this.HiveType = HiveTypeEnum.Sam;
                    break;
                case "security":
                    this.HiveType = HiveTypeEnum.Security;
                    break;
                case "software":
                    this.HiveType = HiveTypeEnum.Software;
                    break;
                case "system":
                    this.HiveType = HiveTypeEnum.System;
                    break;
                case "drivers":
                    this.HiveType = HiveTypeEnum.Drivers;
                    break;
                case "usrclass.dat":
                    this.HiveType = HiveTypeEnum.UsrClass;
                    break;
                case "components":
                    this.HiveType = HiveTypeEnum.Components;
                    break;
                case "bcd":
                    this.HiveType = HiveTypeEnum.Bcd;
                    break;
                case "amcache.hve":
                    this.HiveType = HiveTypeEnum.Amcache;
                    break;
                case "syscache.hve":
                    this.HiveType = HiveTypeEnum.Syscache;
                    break;
                default:
                    this.HiveType = HiveTypeEnum.Other;
                    break;
            }

       //     Logger.Trace($"Hive is a {HiveType} hive");

           this.Version = $"{this.Header.MajorVersion}.{this.Header.MinorVersion}";

        //    Logger.Trace($"Hive version is {version}");
        }

        public bool ParseLog()
        {
            if (this._parsed)
            {
                throw new Exception("ParseLog already called");
            }

            var index = 0x200; //data starts at offset 512 decimal

            while (index < this.FileBytes.Length)
            {
                var sig = Encoding.GetEncoding(1252).GetString(this.FileBytes, index, 4);

                if (sig != "HvLE")
                {
                    //things arent always HvLE as logs get reused, so check to see if we have another valid header at our current offset
                    break;
                }

                var size = BitConverter.ToInt32(this.FileBytes, index + 4);
                var buff = new byte[size];

                Buffer.BlockCopy(this.FileBytes, index, buff, 0, size);

                var tle = new TransactionLogEntry(buff);
                this.TransactionLogEntries.Add(tle);

                index += size;
            }

            this._parsed = true;

            return true;
        }

        /// <summary>
        ///     For the given transaction log, update original hive bytes with the bytes contained in the dirty pages
        /// </summary>
        /// <param name="hiveBytes"></param>
        /// <remarks>This method does nothing to determine IF the data should be overwritten</remarks>
        /// <returns>Byte array containing the updated hive</returns>
        public byte[] UpdateHiveBytes(byte[] hiveBytes)
        {
            const int baseOffset = 0x1000; //hbins start at 4096 bytes

            foreach (var transactionLogEntry in this.TransactionLogEntries)
            {
                if (transactionLogEntry.HasValidHashes() == false)
                {
                    this.Logger.Debug($"Skipping transaction log entry with sequence # 0x{transactionLogEntry.SequenceNumber:X}. Hash verification failed");
                    continue;
                }
                //     Logger.Trace($"Processing log entry: {transactionLogEntry}");

                this.NewSequenceNumber = transactionLogEntry.SequenceNumber;

                foreach (var dirtyPage in transactionLogEntry.DirtyPages)
                {
                    //     Logger.Trace($"Processing dirty page: {dirtyPage}");

                    Buffer.BlockCopy(dirtyPage.PageBytes, 0, hiveBytes, dirtyPage.Offset + baseOffset, dirtyPage.Size);
                }
            }

            return hiveBytes;
        }

        public bool HasValidSignature()
        {
            var sig = BitConverter.ToInt32(this.FileBytes, 0);

            return sig.Equals(RegfSignature);
        }

        public override string ToString()
        {
            var x = 0;
            var sb = new StringBuilder();
            sb.AppendLine();
            foreach (var transactionLogEntry in this.TransactionLogEntries)
            {
                sb.AppendLine($"LogEntry #{x} {transactionLogEntry}");
                x += 1;
            }

            return
                $"Log path: {this.LogPath} Valid checksum: {this.Header.ValidateCheckSum()} primary: 0x{this.Header.PrimarySequenceNumber:X} secondary: 0x{this.Header.SecondarySequenceNumber:X} Entries count: {this.TransactionLogEntries.Count:N0} Entry info: {sb}";
        }
    }
}