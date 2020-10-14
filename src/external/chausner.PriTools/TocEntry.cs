using System.IO;

namespace PriFormat
{
    public class TocEntry
    {
        public string SectionIdentifier { get; }
        public ushort Flags { get; }
        public ushort SectionFlags { get; }
        public uint SectionQualifier { get; }
        public uint SectionOffset { get; }
        public uint SectionLength { get; }

        private TocEntry(string sectionIdentifier, ushort flags, ushort sectionFlags, uint sectionQualifier, uint sectionOffset, uint sectionLength)
        {
            SectionIdentifier = sectionIdentifier;
            Flags = flags;
            SectionFlags = sectionFlags;
            SectionQualifier = sectionQualifier;
            SectionOffset = sectionOffset;
            SectionLength = sectionLength;
        }

        internal static TocEntry Parse(BinaryReader binaryReader)
        {
            return new TocEntry(
                new string(binaryReader.ReadChars(16)),
                binaryReader.ReadUInt16(),
                binaryReader.ReadUInt16(),
                binaryReader.ReadUInt32(),
                binaryReader.ReadUInt32(),
                binaryReader.ReadUInt32());
        }

        public override string ToString()
        {
            return $"{SectionIdentifier.TrimEnd('\0', ' ')} length: {SectionLength}";
        }
    }
}
