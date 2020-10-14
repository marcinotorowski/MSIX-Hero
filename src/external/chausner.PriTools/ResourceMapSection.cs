using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PriFormat
{
    public class ResourceMapSection : Section
    {
        public HierarchicalSchemaReference HierarchicalSchemaReference { get; private set; }
        public SectionRef<HierarchicalSchemaSection> SchemaSection { get; private set; }
        public SectionRef<DecisionInfoSection> DecisionInfoSection { get; private set; }
        public IReadOnlyDictionary<ushort, CandidateSet> CandidateSets { get; private set; }

        bool version2;

        internal const string Identifier1 = "[mrm_res_map__]\0";
        internal const string Identifier2 = "[mrm_res_map2_]\0";

        internal ResourceMapSection(PriFile priFile, bool version2) : base(version2 ? Identifier2 : Identifier1, priFile)
        {
            this.version2 = version2;
        }

        protected override bool ParseSectionContent(BinaryReader binaryReader)
        {
            long sectionPosition = (binaryReader.BaseStream as SubStream)?.SubStreamPosition ?? 0;

            ushort environmentReferencesLength = binaryReader.ReadUInt16();
            ushort numEnvironmentReferences = binaryReader.ReadUInt16();
            if (!version2)
            {
                if (environmentReferencesLength == 0 || numEnvironmentReferences == 0)
                    throw new InvalidDataException();
            }
            else
            {
                if (environmentReferencesLength != 0 || numEnvironmentReferences != 0)
                    throw new InvalidDataException();
            }
            SchemaSection = new SectionRef<HierarchicalSchemaSection>(binaryReader.ReadUInt16());
            ushort hierarchicalSchemaReferenceLength = binaryReader.ReadUInt16();
            DecisionInfoSection = new SectionRef<DecisionInfoSection>(binaryReader.ReadUInt16());
            ushort resourceValueTypeTableSize = binaryReader.ReadUInt16();
            ushort ItemToItemInfoGroupCount = binaryReader.ReadUInt16();
            ushort itemInfoGroupCount = binaryReader.ReadUInt16();
            uint itemInfoCount = binaryReader.ReadUInt32();
            uint numCandidates = binaryReader.ReadUInt32();
            uint dataLength = binaryReader.ReadUInt32();
            uint largeTableLength = binaryReader.ReadUInt32();

            if (PriFile.GetSectionByRef(DecisionInfoSection) == null)
                return false;

            byte[] environmentReferencesData = binaryReader.ReadBytes(environmentReferencesLength);

            byte[] schemaReferenceData = binaryReader.ReadBytes(hierarchicalSchemaReferenceLength);

            if (schemaReferenceData.Length != 0)
                using (BinaryReader r = new BinaryReader(new MemoryStream(schemaReferenceData, false)))
                {
                    ushort majorVersion = r.ReadUInt16();
                    ushort minorVersion = r.ReadUInt16();
                    r.ExpectUInt32(0);
                    uint checksum = r.ReadUInt32();
                    uint numScopes = r.ReadUInt32();
                    uint numItems = r.ReadUInt32();

                    HierarchicalSchemaVersionInfo versionInfo = new HierarchicalSchemaVersionInfo(majorVersion, minorVersion, checksum, numScopes, numItems);

                    ushort stringDataLength = r.ReadUInt16();
                    r.ExpectUInt16(0);
                    uint unknown1 = r.ReadUInt32();
                    uint unknown2 = r.ReadUInt32();
                    string uniqueName = r.ReadNullTerminatedString(Encoding.Unicode);

                    if (uniqueName.Length != stringDataLength - 1)
                        throw new InvalidDataException();

                    HierarchicalSchemaReference = new HierarchicalSchemaReference(versionInfo, unknown1, unknown2, uniqueName);
                }

            List<ResourceValueType> resourceValueTypeTable = new List<ResourceValueType>(resourceValueTypeTableSize);
            for (int i = 0; i < resourceValueTypeTableSize; i++)
            {
                binaryReader.ExpectUInt32(4);
                ResourceValueType resourceValueType = (ResourceValueType)binaryReader.ReadUInt32();
                resourceValueTypeTable.Add(resourceValueType);
            }

            List<ItemToItemInfoGroup> itemToItemInfoGroups = new List<ItemToItemInfoGroup>();
            for (int i = 0; i < ItemToItemInfoGroupCount; i++)
            {
                ushort firstItem = binaryReader.ReadUInt16();
                ushort itemInfoGroup = binaryReader.ReadUInt16();
                itemToItemInfoGroups.Add(new ItemToItemInfoGroup(firstItem, itemInfoGroup));
            }

            List<ItemInfoGroup> itemInfoGroups = new List<ItemInfoGroup>();
            for (int i = 0; i < itemInfoGroupCount; i++)
            {
                ushort groupSize = binaryReader.ReadUInt16();
                ushort firstItemInfo = binaryReader.ReadUInt16();
                itemInfoGroups.Add(new ItemInfoGroup(groupSize, firstItemInfo));
            }

            List<ItemInfo> itemInfos = new List<ItemInfo>();
            for (int i = 0; i < itemInfoCount; i++)
            {
                ushort decision = binaryReader.ReadUInt16();
                ushort firstCandidate = binaryReader.ReadUInt16();
                itemInfos.Add(new ItemInfo(decision, firstCandidate));
            }

            byte[] largeTable = binaryReader.ReadBytes((int)largeTableLength);

            if (largeTable.Length != 0)
                using (BinaryReader r = new BinaryReader(new MemoryStream(largeTable, false)))
                {
                    uint ItemToItemInfoGroupCountLarge = r.ReadUInt32();
                    uint itemInfoGroupCountLarge = r.ReadUInt32();
                    uint itemInfoCountLarge = r.ReadUInt32();

                    for (int i = 0; i < ItemToItemInfoGroupCountLarge; i++)
                    {
                        uint firstItem = r.ReadUInt32();
                        uint itemInfoGroup = r.ReadUInt32();
                        itemToItemInfoGroups.Add(new ItemToItemInfoGroup(firstItem, itemInfoGroup));
                    }

                    for (int i = 0; i < itemInfoGroupCountLarge; i++)
                    {
                        uint groupSize = r.ReadUInt32();
                        uint firstItemInfo = r.ReadUInt32();
                        itemInfoGroups.Add(new ItemInfoGroup(groupSize, firstItemInfo));
                    }

                    for (int i = 0; i < itemInfoCountLarge; i++)
                    {
                        uint decision = r.ReadUInt32();
                        uint firstCandidate = r.ReadUInt32();
                        itemInfos.Add(new ItemInfo(decision, firstCandidate));
                    }

                    if (r.BaseStream.Position != r.BaseStream.Length)
                        throw new InvalidDataException();
                }

            List<CandidateInfo> candidateInfos = new List<CandidateInfo>((int)numCandidates);
            for (int i = 0; i < numCandidates; i++)
            {
                byte type = binaryReader.ReadByte();

                if (type == 0x01)
                {
                    ResourceValueType resourceValueType = resourceValueTypeTable[binaryReader.ReadByte()];
                    ushort sourceFileIndex = binaryReader.ReadUInt16();
                    ushort valueLocation = binaryReader.ReadUInt16();
                    ushort dataItemSection = binaryReader.ReadUInt16();
                    candidateInfos.Add(new CandidateInfo(resourceValueType, sourceFileIndex, valueLocation, dataItemSection));
                }
                else if (type == 0x00)
                {
                    ResourceValueType resourceValueType = resourceValueTypeTable[binaryReader.ReadByte()];
                    ushort length = binaryReader.ReadUInt16();
                    uint stringOffset = binaryReader.ReadUInt32();
                    candidateInfos.Add(new CandidateInfo(resourceValueType, length, stringOffset));
                }
                else
                    throw new InvalidDataException();
            }

            long stringDataStartOffset = binaryReader.BaseStream.Position;

            Dictionary<ushort, CandidateSet> candidateSets = new Dictionary<ushort, CandidateSet>();

            for (int itemToItemInfoGroupIndex = 0; itemToItemInfoGroupIndex < itemToItemInfoGroups.Count; itemToItemInfoGroupIndex++)
            {
                ItemToItemInfoGroup itemToItemInfoGroup = itemToItemInfoGroups[itemToItemInfoGroupIndex];

                ItemInfoGroup itemInfoGroup;

                if (itemToItemInfoGroup.ItemInfoGroup < itemInfoGroups.Count)
                    itemInfoGroup = itemInfoGroups[(int)itemToItemInfoGroup.ItemInfoGroup];
                else
                    itemInfoGroup = new ItemInfoGroup(1, (uint)(itemToItemInfoGroup.ItemInfoGroup - itemInfoGroups.Count));

                for (uint itemInfoIndex = itemInfoGroup.FirstItemInfo; itemInfoIndex < itemInfoGroup.FirstItemInfo + itemInfoGroup.GroupSize; itemInfoIndex++)
                {
                    ItemInfo itemInfo = itemInfos[(int)itemInfoIndex];

                    ushort decisionIndex = (ushort)itemInfo.Decision;

                    Decision decision = PriFile.GetSectionByRef(DecisionInfoSection).Decisions[decisionIndex];

                    List<Candidate> candidates = new List<Candidate>(decision.QualifierSets.Count);

                    for (int i = 0; i < decision.QualifierSets.Count; i++)
                    {
                        CandidateInfo candidateInfo = candidateInfos[(int)itemInfo.FirstCandidate + i];

                        if (candidateInfo.Type == 0x01)
                        {
                            ReferencedFileRef? sourceFile;

                            if (candidateInfo.SourceFileIndex == 0)
                                sourceFile = null;
                            else
                                sourceFile = new ReferencedFileRef(candidateInfo.SourceFileIndex - 1);

                            candidates.Add(new Candidate(decision.QualifierSets[i].Index, candidateInfo.ResourceValueType, sourceFile,
                                new DataItemRef(new SectionRef<DataItemSection>(candidateInfo.DataItemSection), candidateInfo.DataItemIndex)));
                        }
                        else if (candidateInfo.Type == 0x00)
                        {
                            ByteSpan data = new ByteSpan(sectionPosition + stringDataStartOffset + candidateInfo.DataOffset, candidateInfo.DataLength);

                            candidates.Add(new Candidate(decision.QualifierSets[i].Index, candidateInfo.ResourceValueType, data));
                        }
                    }

                    ushort resourceMapItemIndex = (ushort)(itemToItemInfoGroup.FirstItem + (itemInfoIndex - itemInfoGroup.FirstItemInfo));

                    CandidateSet candidateSet = new CandidateSet(
                        new ResourceMapItemRef(SchemaSection, resourceMapItemIndex),
                        decisionIndex,
                        candidates);

                    candidateSets.Add(resourceMapItemIndex, candidateSet);
                }
            }

            CandidateSets = candidateSets;

            return true;
        }

        private struct ItemToItemInfoGroup
        {
            public uint FirstItem;
            public uint ItemInfoGroup;

            public ItemToItemInfoGroup(uint firstItem, uint itemInfoGroup)
            {
                FirstItem = firstItem;
                ItemInfoGroup = itemInfoGroup;
            }
        }

        private struct ItemInfoGroup
        {
            public uint GroupSize;
            public uint FirstItemInfo;

            public ItemInfoGroup(uint groupSize, uint firstItemInfo)
            {
                GroupSize = groupSize;
                FirstItemInfo = firstItemInfo;
            }
        }

        private struct ItemInfo
        {
            public uint Decision;
            public uint FirstCandidate;

            public ItemInfo(uint decision, uint firstCandidate)
            {
                Decision = decision;
                FirstCandidate = firstCandidate;
            }
        }

        private struct CandidateInfo
        {
            public byte Type;
            public ResourceValueType ResourceValueType;

            // Type 1
            public ushort SourceFileIndex;
            public ushort DataItemIndex;
            public ushort DataItemSection;

            // Type 0
            public ushort DataLength;
            public uint DataOffset;

            public CandidateInfo(ResourceValueType resourceValueType, ushort sourceFileIndex, ushort dataItemIndex, ushort dataItemSection)
            {
                Type = 0x01;
                ResourceValueType = resourceValueType;
                SourceFileIndex = sourceFileIndex;
                DataItemIndex = dataItemIndex;
                DataItemSection = dataItemSection;
                DataLength = 0;
                DataOffset = 0;
            }

            public CandidateInfo(ResourceValueType resourceValueType, ushort dataLength, uint dataOffset)
            {
                Type = 0x00;
                ResourceValueType = resourceValueType;
                SourceFileIndex = 0;
                DataItemIndex = 0;
                DataItemSection = 0;
                DataLength = dataLength;
                DataOffset = dataOffset;
            }
        }
    }

    public enum ResourceValueType
    {
        String,
        Path,
        EmbeddedData,
        AsciiString,
        Utf8String,
        AsciiPath,
        Utf8Path
    }

    public class CandidateSet
    {
        public ResourceMapItemRef ResourceMapItem { get; }
        public ushort DecisionIndex { get; }
        public IReadOnlyList<Candidate> Candidates { get; }

        internal CandidateSet(ResourceMapItemRef resourceMapItem, ushort decisionIndex, IReadOnlyList<Candidate> candidates)
        {
            ResourceMapItem = resourceMapItem;
            DecisionIndex = decisionIndex;
            Candidates = candidates;
        }
    }

    public class Candidate
    {
        public ushort QualifierSet { get; }
        public ResourceValueType Type { get; }
        public ReferencedFileRef? SourceFile { get; }
        public DataItemRef? DataItem { get; }
        public ByteSpan? Data { get; }

        internal Candidate(ushort qualifierSet, ResourceValueType type, ReferencedFileRef? sourceFile, DataItemRef dataItem)
        {
            QualifierSet = qualifierSet;
            Type = type;
            SourceFile = sourceFile;
            DataItem = dataItem;
            Data = null;
        }

        internal Candidate(ushort qualifierSet, ResourceValueType type, ByteSpan data)
        {
            QualifierSet = qualifierSet;
            Type = type;
            SourceFile = null;
            DataItem = null;
            Data = data;
        }
    }

    public class HierarchicalSchemaReference
    {
        public HierarchicalSchemaVersionInfo VersionInfo { get; }
        public uint Unknown1 { get; }
        public uint Unknown2 { get; }
        public string UniqueName { get; }

        internal HierarchicalSchemaReference(HierarchicalSchemaVersionInfo versionInfo, uint unknown1, uint unknown2, string uniqueName)
        {
            VersionInfo = versionInfo;
            Unknown1 = unknown1;
            Unknown2 = unknown2;
            UniqueName = uniqueName;
        }
    }

    public struct ResourceMapItemRef
    {
        internal SectionRef<HierarchicalSchemaSection> schemaSection;
        internal int itemIndex;

        internal ResourceMapItemRef(SectionRef<HierarchicalSchemaSection> schemaSection, int itemIndex)
        {
            this.schemaSection = schemaSection;
            this.itemIndex = itemIndex;
        }
    }
}
