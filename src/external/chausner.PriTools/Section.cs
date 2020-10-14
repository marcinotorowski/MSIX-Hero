using System;
using System.IO;
using System.Text;

namespace PriFormat
{
    public abstract class Section
    {
        protected PriFile PriFile { get; private set; }
        public string SectionIdentifier { get; private set; }
        public uint SectionQualifier { get; private set; }
        public uint Flags { get; private set; }
        public uint SectionFlags { get; private set; }
        public uint SectionLength { get; private set; }

        protected Section(string sectionIdentifier, PriFile priFile)
        {
            if (sectionIdentifier.Length != 16)
                throw new ArgumentException("Section identifiers need to be exactly 16 characters long.", nameof(sectionIdentifier));

            SectionIdentifier = sectionIdentifier;
            PriFile = priFile;
        }

        internal bool Parse(BinaryReader binaryReader)
        {
            if (new string(binaryReader.ReadChars(16)) != SectionIdentifier)
                throw new InvalidDataException("Unexpected section identifier.");

            SectionQualifier = binaryReader.ReadUInt32();
            Flags = binaryReader.ReadUInt16();
            SectionFlags = binaryReader.ReadUInt16();
            SectionLength = binaryReader.ReadUInt32();
            binaryReader.ExpectUInt32(0);

            binaryReader.BaseStream.Seek(SectionLength - 16 - 24, SeekOrigin.Current);

            binaryReader.ExpectUInt32(0xDEF5FADE);
            binaryReader.ExpectUInt32(SectionLength);

            binaryReader.BaseStream.Seek(-8 - (SectionLength - 16 - 24), SeekOrigin.Current);

            using (SubStream subStream = new SubStream(binaryReader.BaseStream,binaryReader.BaseStream.Position, (int)SectionLength - 16 - 24))
            using (BinaryReader subBinaryReader = new BinaryReader(subStream, Encoding.ASCII))
            {
                return ParseSectionContent(subBinaryReader);
            }
        }

        protected abstract bool ParseSectionContent(BinaryReader binaryReader);

        public override string ToString()
        {
            return $"{SectionIdentifier.TrimEnd('\0', ' ')} length: {SectionLength}";
        }

        internal static Section CreateForIdentifier(string sectionIdentifier, PriFile priFile)
        {
            switch (sectionIdentifier)
            {
                case PriDescriptorSection.Identifier:
                    return new PriDescriptorSection(priFile);
                case HierarchicalSchemaSection.Identifier1:
                    return new HierarchicalSchemaSection(priFile, false);
                case HierarchicalSchemaSection.Identifier2:
                    return new HierarchicalSchemaSection(priFile, true);
                case DecisionInfoSection.Identifier:
                    return new DecisionInfoSection(priFile);
                case ResourceMapSection.Identifier1:
                    return new ResourceMapSection(priFile, false);
                case ResourceMapSection.Identifier2:
                    return new ResourceMapSection(priFile, true);
                case DataItemSection.Identifier:
                    return new DataItemSection(priFile);
                case ReverseMapSection.Identifier:
                    return new ReverseMapSection(priFile);
                case ReferencedFileSection.Identifier:
                    return new ReferencedFileSection(priFile);
                default:
                    return new UnknownSection(sectionIdentifier, priFile);
            }
        }
    }
}
