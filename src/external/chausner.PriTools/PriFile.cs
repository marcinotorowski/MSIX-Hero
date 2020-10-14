using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PriFormat
{
    public class PriFile
    {
        public string Version { get; private set; }
        public uint TotalFileSize { get; private set; }
        public IReadOnlyList<TocEntry> TableOfContents { get; private set; }
        public IReadOnlyList<Section> Sections { get; private set; }

        private PriFile()
        {
        }

        public static PriFile Parse(Stream stream)
        {
            PriFile priFile = new PriFile();

            priFile.ParseInternal(stream);

            return priFile;
        }

        private void ParseInternal(Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                long fileStartOffset = binaryReader.BaseStream.Position;

                string magic = new string(binaryReader.ReadChars(8));

                switch (magic)
                {
                    case "mrm_pri0":
                    case "mrm_pri1":
                    case "mrm_pri2":
                    case "mrm_prif":
                        Version = magic;
                        break;
                    default:
                        throw new InvalidDataException("Data does not start with a PRI file header.");
                }

                binaryReader.ExpectUInt16(0);
                binaryReader.ExpectUInt16(1);
                TotalFileSize = binaryReader.ReadUInt32();
                uint tocOffset = binaryReader.ReadUInt32();
                uint sectionStartOffset = binaryReader.ReadUInt32();
                ushort numSections = binaryReader.ReadUInt16();

                binaryReader.ExpectUInt16(0xFFFF);
                binaryReader.ExpectUInt32(0);

                binaryReader.BaseStream.Seek(fileStartOffset + TotalFileSize - 16, SeekOrigin.Begin);

                binaryReader.ExpectUInt32(0xDEFFFADE);
                binaryReader.ExpectUInt32(TotalFileSize);
                binaryReader.ExpectString(magic);

                binaryReader.BaseStream.Seek(tocOffset, SeekOrigin.Begin);

                List<TocEntry> toc = new List<TocEntry>(numSections);

                for (int i = 0; i < numSections; i++)
                    toc.Add(TocEntry.Parse(binaryReader));

                TableOfContents = toc;

                Section[] sections = new Section[numSections];

                Sections = sections;

                bool parseSuccess = false;
                bool parseFailure = false;

                do
                {
                    for (int i = 0; i < sections.Length; i++)
                        if (sections[i] == null)
                        {
                            binaryReader.BaseStream.Seek(sectionStartOffset + toc[i].SectionOffset, SeekOrigin.Begin);

                            Section section = Section.CreateForIdentifier(toc[i].SectionIdentifier, this);

                            if (section.Parse(binaryReader))
                            {
                                sections[i] = section;
                                parseSuccess = true;
                            }
                            else
                                parseFailure = true;
                        }
                } while (parseFailure && parseSuccess);

                if (parseFailure)
                    throw new InvalidDataException();
            }
        }

        PriDescriptorSection priDescriptorSection;

        public PriDescriptorSection PriDescriptorSection
        {
            get
            {
                if (priDescriptorSection == null)
                    priDescriptorSection = Sections.OfType<PriDescriptorSection>().Single();

                return priDescriptorSection;
            }
        }

        public T GetSectionByRef<T>(SectionRef<T> sectionRef) where T : Section
        {
            return (T)Sections[sectionRef.sectionIndex];
        }

        public ResourceMapItem GetResourceMapItemByRef(ResourceMapItemRef resourceMapItemRef)
        {
            return GetSectionByRef(resourceMapItemRef.schemaSection).Items[resourceMapItemRef.itemIndex];
        }

        public ByteSpan GetDataItemByRef(DataItemRef dataItemRef)
        {
            return GetSectionByRef(dataItemRef.dataItemSection).DataItems[dataItemRef.itemIndex];
        }

        public ReferencedFile GetReferencedFileByRef(ReferencedFileRef referencedFileRef)
        {
            return GetSectionByRef(PriDescriptorSection.ReferencedFileSections.First()).ReferencedFiles[referencedFileRef.fileIndex];
        }
    }
}
