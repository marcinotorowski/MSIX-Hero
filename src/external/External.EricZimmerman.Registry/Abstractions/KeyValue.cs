

// namespaces...

namespace External.EricZimmerman.Registry.Abstractions
{
    using System;
    using System.Text;

    using External.EricZimmerman.Registry.Cells;

    // public classes...
    /// <summary>
    ///     Represents a value that is associated with a RegistryKey
    ///     <remarks>Also contains references to low level structures related to a given value</remarks>
    /// </summary>
    public class KeyValue
    {
        // public constructors...
        /// <summary>
        ///     Initializes a new instance of the <see cref="KeyValue" /> class.
        /// </summary>
        public KeyValue(VkCellRecord vk)
        {
            this.VkRecord = vk;
            this.InternalGuid = Guid.NewGuid().ToString();
        }

        // public properties...
        /// <summary>
        ///     A unique value that can be used to find this key in a collection
        /// </summary>
        public string InternalGuid { get; }

        /// <summary>
        ///     The normalized representation of the value's value.
        /// </summary>
        public string ValueData
        {
            get
            {
                if (this.VkRecord.ValueData is byte[])
                {
                    return BitConverter.ToString((byte[]) this.VkRecord.ValueData);
                }

                return this.VkRecord.ValueData.ToString();
            }
        }

        /// <summary>
        ///     The value as stored in the hive as a series of bytes
        /// </summary>
        public byte[] ValueDataRaw => this.VkRecord.ValueDataRaw;

        public string ValueName => this.VkRecord.ValueName;

        /// <summary>
        ///     If present, the value slack as a string of bytes delimited by hyphens
        /// </summary>
        public string ValueSlack => BitConverter.ToString(this.VkRecord.ValueDataSlack);

        /// <summary>
        ///     The value slack as stored in the hive as a series of bytes
        /// </summary>
        public byte[] ValueSlackRaw => this.VkRecord.ValueDataSlack;

        /// <summary>
        ///     The values type (VKCellRecord.DataTypeEnum)
        /// </summary>
        public string ValueType => this.VkRecord.DataType.ToString();

        /// <summary>
        ///     The underlying VKRecord for this Key. This allows access to all info about the VK Record
        /// </summary>
        public VkCellRecord VkRecord { get; }

        // public methods...
        public override string ToString()
        {
            var sb = new StringBuilder();

//            sb.AppendLine($"Value Name: {ValueName}");
//            sb.AppendLine($"Value Type: {ValueType}");
//            sb.AppendLine($"Value Data: {ValueData}");
//            sb.AppendLine($"Value Slack: {ValueSlack}");
//
//            sb.AppendLine();

//            sb.AppendLine(string.Format("Internal GUID: {0}", InternalGUID));
//            sb.AppendLine();

            sb.AppendLine($"VK Record: {this.VkRecord}");

            return sb.ToString();
        }
    }
}