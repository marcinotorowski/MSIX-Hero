using System.IO;

namespace PriFormat
{
    public class UnknownSection : Section
    {
        public byte[] SectionContent { get; private set; }

        internal UnknownSection(string sectionIdentifier, PriFile priFile) : base(sectionIdentifier, priFile)
        {
        }

        protected override bool ParseSectionContent(BinaryReader binaryReader)
        {
            int contentLength = (int)(binaryReader.BaseStream.Length - binaryReader.BaseStream.Position);

            SectionContent = binaryReader.ReadBytes(contentLength);

            return true;
        }
    }
}
