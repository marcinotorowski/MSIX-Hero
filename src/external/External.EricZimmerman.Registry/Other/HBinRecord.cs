using static External.EricZimmerman.Registry.Other.Helpers;

// namespaces...

namespace External.EricZimmerman.Registry.Other
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    using External.EricZimmerman.Registry.Cells;
    using External.EricZimmerman.Registry.Lists;

    // public classes...
    public class HBinRecord
    {
        private readonly int _minorVersion;
        private readonly bool _recoverDeleted;

        private readonly RegistryHive _registryHive;
        private byte[] _rawBytes;

        // protected internal constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="HBinRecord" /> class.
        ///     <remarks>Represents a Hive Bin Record</remarks>
        /// </summary>
        protected internal HBinRecord(byte[] rawBytes, long relativeOffset, int minorVersion, bool recoverDeleted,
            RegistryHive reg)
        {
            this.RelativeOffset = relativeOffset;

            this._registryHive = reg;

            this._recoverDeleted = recoverDeleted;

            this._minorVersion = minorVersion;

            this._rawBytes = rawBytes;

            this.Signature = Encoding.ASCII.GetString(rawBytes, 0, 4);

            var sig = BitConverter.ToInt32(rawBytes, 0);

           // Check.That(sig).IsEqualTo(HbinSignature);
            if (sig != HbinSignature)
            {
                throw new Exception("Invalid hbin signature");
            }

          //  reg.Logger.Trace("Got valid hbin signature for hbin at absolute offset 0x{0:X}", AbsoluteOffset);

            this.FileOffset = BitConverter.ToUInt32(rawBytes, 0x4);

            this.Size = BitConverter.ToUInt32(rawBytes, 0x8);

            this.Reserved = BitConverter.ToUInt32(rawBytes, 0xc);

            var ts = BitConverter.ToInt64(rawBytes, 0x14);

            try
            {
                var dt = DateTimeOffset.FromFileTime(ts).ToUniversalTime();

                if (dt.Year > 1601)
                {
                    this.LastWriteTimestamp = dt;
                }
            }
            catch (Exception) //ncrunch: no coverage
            {
                //ncrunch: no coverage
                //very rarely you get a 'Not a valid Win32 FileTime' error, so trap it if thats the case
            } //ncrunch: no coverage

            this.Spare = BitConverter.ToUInt32(rawBytes, 0xc);
        }

        // public properties...
        /// <summary>
        ///     The offset to this record from the beginning of the hive, in bytes
        /// </summary>
        public long AbsoluteOffset => this.RelativeOffset + 4096;

        // public properties...
        /// <summary>
        ///     The relative offset to this record
        /// </summary>
        public uint FileOffset { get; }

        /// <summary>
        ///     The last write time of this key
        /// </summary>
        public DateTimeOffset? LastWriteTimestamp { get; }

        /// <summary>
        ///     The offset to this record as stored by other records
        ///     <remarks>This value will be 4096 bytes (the size of the regf header) less than the AbsoluteOffset</remarks>
        /// </summary>
        public long RelativeOffset { get; }

        public uint Reserved { get; }

        /// <summary>
        ///     The signature of the hbin record. Should always be "hbin"
        /// </summary>
        public string Signature { get; }

        /// <summary>
        ///     The size of the hive
        ///     <remarks>
        ///         This value will always be positive. See IsFree to determine whether or not this cell is in use (it has a
        ///         negative size)
        ///     </remarks>
        /// </summary>
        public uint Size { get; }

        public uint Spare { get; }

        public List<IRecordBase> Process()
        {
            var records = new List<IRecordBase>();

            //additional cell data starts 32 bytes (0x20) in
            var offsetInHbin = 0x20;

            this._registryHive.TotalBytesRead += 0x20;

            while (offsetInHbin < this.Size)
            {
                var recordSize = BitConverter.ToUInt32(this._rawBytes, offsetInHbin);

                var readSize = (int) recordSize;

                if (!this._recoverDeleted && readSize > 0)
                {
                    //since we do not want to get deleted stuff, if the cell size > 0, its free, so skip it
                    offsetInHbin += readSize;
                    continue;
                }

                // if we get a negative number here the record is allocated, but we cant read negative bytes, so get absolute value
                readSize = Math.Abs(readSize);

     //           _registryHive.Logger.Trace(
     //               $"Getting rawRecord at hbin relative offset 0x{offsetInHbin:X} (Absolute offset: 0x{offsetInHbin + RelativeOffset + 0x1000:X}). readsize: {readSize}");

                var rawRecord = new ArraySegment<byte>(this._rawBytes, offsetInHbin, readSize).ToArray();

                this._registryHive.TotalBytesRead += readSize;

                var cellSignature = Encoding.ASCII.GetString(rawRecord, 4, 2);
                var cellSignature2 = BitConverter.ToInt16(rawRecord, 4);

                //ncrunch: no coverage start
                if (this._registryHive.Logger.IsDebugEnabled)
                {
                    var foundMatch = false;

                    foundMatch = Regex.IsMatch(cellSignature, @"\A[a-z]{2}\z");

                    //only process records with 2 letter signatures. this avoids crazy output for data cells
                    if (foundMatch)
                    {
                        //                 _registryHive.Logger.Trace(
                        //                      $"Processing {cellSignature} record at hbin relative offset 0x{offsetInHbin:X} (Absolute offset: 0x{offsetInHbin + RelativeOffset + 0x1000:X})");
                    }
                    else
                    {
                        //                 _registryHive.Logger.Trace(
                        //                      $"Processing data record at hbin relative offset 0x{offsetInHbin:X} (Absolute offset: 0x{offsetInHbin + RelativeOffset + 0x1000:X})");
                    }
                }
                //ncrunch: no coverage end

                ICellTemplate cellRecord = null;
                IListTemplate listRecord = null;
                DataNode dataRecord = null;

                

                try
                {
                    switch (cellSignature2)
                    {
                        case LfSignature:
                        case LhSignature:
                            listRecord = new LxListRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            break;

                        case LiSignature:
                            listRecord = new LiListRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            break;

                        case RiSignature:
                            listRecord = new RiListRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            break;

                        case DbSignature:
                            listRecord = new DbListRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            break;

                        case LkSignature:
                            cellRecord = new LkCellRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            break; //ncrunch: no coverage

                        case NkSignature:
                            if (rawRecord.Length >= 0x30) // the minimum length for a recoverable record
                            {
                                cellRecord = new NkCellRecord(rawRecord.Length, offsetInHbin + this.RelativeOffset,
                                    this._registryHive);
                            }

                            break;
                        case SkSignature:
                            if (rawRecord.Length >= 0x14) // the minimum length for a recoverable record
                            {
                                cellRecord = new SkCellRecord(rawRecord, offsetInHbin + this.RelativeOffset);
                            }

                            break;

                        case VkSignature:
                            if (rawRecord.Length >= 0x18) // the minimum length for a recoverable record
                            {
                                cellRecord = new VkCellRecord(rawRecord.Length, offsetInHbin + this.RelativeOffset,
                                    this._minorVersion, this._registryHive);
                            }

                            break;

                        default:
                            dataRecord = new DataNode(rawRecord, offsetInHbin + this.RelativeOffset);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //check size and see if its free. if so, dont worry about it. too small to be of value, but store it somewhere else?

                    var size = BitConverter.ToInt32(rawRecord, 0);

                    if (size < 0)
                    {
                        //ncrunch: no coverage
                        RegistryHive.HardParsingErrorsInternal += 1; //ncrunch: no coverage

                        this._registryHive.Logger.Error( //ncrunch: no coverage     
                            ex,
                            $"Hard error processing record with cell signature {cellSignature} at Absolute Offset: 0x{offsetInHbin + this.RelativeOffset + 4096:X} with raw data: {BitConverter.ToString(rawRecord)}");

                        //TODO store it somewhere else as a placeholder if its in use. include relative offset and other critical stuff
                    } //ncrunch: no coverage                     
                    else
                    {
                        this._registryHive.Logger.Warn(
                            ex,
                            $"Soft error processing record with cell signature {cellSignature} at Absolute Offset: 0x{offsetInHbin + this.RelativeOffset + 4096:X} with raw data: {BitConverter.ToString(rawRecord)}");
                        //This record is marked 'Free' so its not as important of an error
                        RegistryHive.SoftParsingErrorsInternal += 1;
                    }
                }

                List<IRecordBase> carvedRecords = null;

                if (cellRecord != null)
                {
                    if (cellRecord.IsFree)
                    {
                        carvedRecords = this.ExtractRecordsFromSlack(cellRecord.RawBytes, cellRecord.RelativeOffset);
                    }
                    else
                    {
                        records.Add((IRecordBase) cellRecord);
                    }
                }

                if (listRecord != null)
                {
                    if (this._recoverDeleted)
                    {
                        carvedRecords = this.ExtractRecordsFromSlack(listRecord.RawBytes, listRecord.RelativeOffset);
                    }

                    records.Add((IRecordBase) listRecord);
                }

                if (dataRecord != null && this._recoverDeleted)
                {
                    carvedRecords = this.ExtractRecordsFromSlack(dataRecord.RawBytes, dataRecord.RelativeOffset);
                }

                if (carvedRecords != null)
                {
                    if (carvedRecords.Count > 0)
                    {
                        records.AddRange(carvedRecords);
                    }
                }

                offsetInHbin += readSize;
            }

            this._rawBytes = null;
            return records;
        }

        private List<IRecordBase> ExtractRecordsFromSlack(byte[] remainingData, long relativeoffset)
        {
            var records = new List<IRecordBase>();

//            if (remainingData.Length == 4064 && _registryHive.HivePath.Contains("DeletedBags"))
//            {
//                Debug.WriteLine(1);
//            }

            var offsetList2 = new List<int>();

            byte[] raw = null;

         //   _registryHive.Logger.Trace("Looking for cell signatures at absolute offset 0x{0:X}",
      //          relativeoffset + 0x1000);

            for (var i = 0; i < remainingData.Length; i++)
            {
                if (remainingData[i] == 0x6b) //6b == k
                {
                    if (remainingData[i - 1] == 0x6e || remainingData[i - 1] == 0x76) //6e = n, 76 = v
                    {
                        //if we are here we have a good signature, nk or vk
                        //check what is before that to see if its 0x00 or 0xFF
                        if (remainingData[i - 2] == 0x00 || remainingData[i - 2] == 0xFF)
                        {
                            //winner! since we initially hit on ZZ, subtract 5 to get to the beginning of the record, XX XX XX XX YY ZZ
                            offsetList2.Add(i - 5);
                        }
                    }
                }
            }

            //offsetList2 now has offset of every record signature we are interested in
            foreach (var i in offsetList2)
            {
                try
                {
                    var actualStart = i;

                    var size = BitConverter.ToInt32(remainingData, actualStart);

                    if (Math.Abs(size) <= 3 || remainingData.Length - actualStart < size)
                    {
                        //if its empty or the size is beyond the data that is left, move on
                        continue;
                    }

                    raw = new ArraySegment<byte>(remainingData, actualStart, Math.Abs(size)).ToArray();

                    if (raw.Length < 6)
                    {
                        continue;
                    }
                    // since we need 4 bytes for the size and 2 for sig, if its smaller than 6, go to next one

                    var sig2 = BitConverter.ToInt16(raw, 4);

                    switch (sig2)
                    {
                        case NkSignature:
                            if (raw.Length <= 0x30)
                            {
                                continue;
                            }

                            var nk = new NkCellRecord(raw.Length, relativeoffset + actualStart, this._registryHive);
                            if (nk.LastWriteTimestamp.Year > 1900)
                            {
                  //              _registryHive.Logger.Trace("Found nk record in slack at absolute offset 0x{0:X}",
              //                      relativeoffset + actualStart + 0x1000);
                                records.Add(nk);
                            }

                            break;
                        case VkSignature:
                            if (raw.Length < 0x18)
                            {
                                //cant have a record shorter than this, even when no name is present
                                continue;
                            }

                            var vk = new VkCellRecord(raw.Length, relativeoffset + actualStart, this._minorVersion,
                                this._registryHive);
                      //      _registryHive.Logger.Trace("Found vk record in slack at absolute offset 0x{0:X}",
                  //              relativeoffset + actualStart + 0x1000);
                            records.Add(vk);
                            break;
                    }
                }
                catch (Exception ex) //ncrunch: no coverage
                {
                    //ncrunch: no coverage
                    // this is a corrupted/unusable record
                    this._registryHive.Logger.Debug( //ncrunch: no coverage
                        ex,
                        $"When recovering from slack at absolute offset 0x{relativeoffset + i + 0x1000:X8}, an error happened! raw Length: 0x{raw?.Length:x}");

                    RegistryHive.SoftParsingErrorsInternal += 1; //ncrunch: no coverage
                } //ncrunch: no coverage
            }


            return records;
        }

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Size: 0x{this.Size:X}");
            sb.AppendLine($"Relative Offset: 0x{this.RelativeOffset:X}");
            sb.AppendLine($"Absolute Offset: 0x{this.AbsoluteOffset:X}");

            sb.AppendLine($"Signature: {this.Signature}");

            if (this.LastWriteTimestamp.HasValue)
            {
                sb.AppendLine($"Last Write Timestamp: {this.LastWriteTimestamp}");
            }

            sb.AppendLine();

            sb.AppendLine();
            sb.AppendLine($"File offset: 0x{this.FileOffset:X}");
            sb.AppendLine();

            sb.AppendLine($"Reserved: 0x{this.Reserved:X}");
            sb.AppendLine($"Spare: 0x{this.Spare:X}");

            return sb.ToString();
        }
    }
}