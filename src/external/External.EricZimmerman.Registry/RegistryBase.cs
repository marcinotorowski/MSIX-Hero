using static External.EricZimmerman.Registry.Other.Helpers;

namespace External.EricZimmerman.Registry
{
    using System;
    using System.IO;
    using System.Text;

    using External.EricZimmerman.Registry.Other;

    using NLog;

    public class RegistryBase : IRegistry
    {
        internal readonly Logger Logger = LogManager.GetCurrentClassLogger();

        

        public RegistryBase()
        {
            throw new NotSupportedException("Call the other constructor and pass in the path to the Registry hive!");
        }

        public RegistryBase(byte[] rawBytes, string hivePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            this.FileBytes = rawBytes;
            this.HivePath = "None";

            this.Logger = LogManager.GetLogger("rawBytes");

            if (!this.HasValidSignature())
            {
                this.Logger.Error("Data in byte array is not a Registry hive (bad signature)");

                throw new ArgumentException("Data in byte array is not a Registry hive (bad signature)");
            }

            this.HivePath = hivePath;

            this.Initialize();
        }

        public RegistryBase(string hivePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (hivePath == null)
            {
                throw new ArgumentNullException("hivePath cannot be null");
            }

            if (!File.Exists(hivePath))
            {
                var fullPath =  Path.GetFullPath(hivePath);
                throw new FileNotFoundException($"The specified file {fullPath} was not found.", fullPath);
            }

            var fileStream = new FileStream(hivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var binaryReader = new BinaryReader(fileStream);

            binaryReader.BaseStream.Seek(0, SeekOrigin.Begin);

            this.FileBytes = binaryReader.ReadBytes((int) binaryReader.BaseStream.Length);

            binaryReader.Close();
            fileStream.Close();

            this.Logger = LogManager.GetLogger(hivePath);

            if (!this.HasValidSignature())
            {
                this.Logger.Error("'{0}' is not a Registry hive (bad signature)", hivePath);

                throw new Exception($"'{hivePath}' is not a Registry hive (bad signature)");
            }

            this.HivePath = hivePath;

        //    Logger.Trace("Set HivePath to {0}", hivePath);

            this.Initialize();
        }

        public long TotalBytesRead { get; internal set; }

        public byte[] FileBytes { get; internal set; }

        public HiveTypeEnum HiveType { get; private set; }

        public string HivePath { get; }
        public string Version { get; private set; }

        public RegistryHeader Header { get; set; }

        public byte[] ReadBytesFromHive(long offset, int length)
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

        internal void Initialize()
        {
            var header = this.ReadBytesFromHive(0, 4096);

        //    Logger.Trace("Getting header");

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

        //    Logger.Trace("Hive is a {0} hive", HiveType);

           this.Version = $"{this.Header.MajorVersion}.{this.Header.MinorVersion}";

         //   Logger.Trace("Hive version is {0}", version);
        }

        public bool HasValidSignature()
        {
            var sig = BitConverter.ToInt32(this.FileBytes, 0);

            return sig.Equals(RegfSignature);
        }
    }
}