using System.Runtime.InteropServices.ComTypes;

namespace OffregLib
{
    /// <summary>
    ///     Object representing all the results from the ORQueryInfoKey call
    /// </summary>
    internal class QueryInfoKeyData
    {
        public string Class { get; set; }
        public uint SubKeysCount { get; set; }

        /// <summary>
        ///     Chars size, excluding null char.
        /// </summary>
        public uint MaxSubKeyLen { get; set; }

        /// <summary>
        ///     Chars size, excluding null char.
        /// </summary>
        public uint MaxClassLen { get; set; }

        public uint ValuesCount { get; set; }

        /// <summary>
        ///     Chars size, excluding null char.
        /// </summary>
        public uint MaxValueNameLen { get; set; }

        public uint MaxValueLen { get; set; }
        public uint SizeSecurityDescriptor { get; set; }
        public FILETIME LastWriteTime { get; set; }
    }
}