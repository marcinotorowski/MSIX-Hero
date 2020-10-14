using System;
using System.Collections.Generic;
using System.IO;

namespace PriFormat
{
    public class PriDescriptorSection : Section
    {
        public PriDescriptorFlags PriFlags { get; private set; }

        public IReadOnlyList<SectionRef<HierarchicalSchemaSection>> HierarchicalSchemaSections { get; private set; }
        public IReadOnlyList<SectionRef<DecisionInfoSection>> DecisionInfoSections { get; private set; }
        public IReadOnlyList<SectionRef<ResourceMapSection>> ResourceMapSections { get; private set; }
        public IReadOnlyList<SectionRef<ReferencedFileSection>> ReferencedFileSections { get; private set; }
        public IReadOnlyList<SectionRef<DataItemSection>> DataItemSections { get; private set; }

        public SectionRef<ResourceMapSection>? PrimaryResourceMapSection { get; private set; }

        internal const string Identifier = "[mrm_pridescex]\0";

        internal PriDescriptorSection(PriFile priFile) : base(Identifier, priFile)
        {
        }

        protected override bool ParseSectionContent(BinaryReader binaryReader)
        {
            PriFlags = (PriDescriptorFlags)binaryReader.ReadUInt16();
            ushort includedFileListSection = binaryReader.ReadUInt16();
            binaryReader.ExpectUInt16(0);
            ushort numHierarchicalSchemaSections = binaryReader.ReadUInt16();
            ushort numDecisionInfoSections = binaryReader.ReadUInt16();
            ushort numResourceMapSections = binaryReader.ReadUInt16();
            ushort primaryResourceMapSection = binaryReader.ReadUInt16();
            if (primaryResourceMapSection != 0xFFFF)
                PrimaryResourceMapSection = new SectionRef<ResourceMapSection>(primaryResourceMapSection);
            else
                PrimaryResourceMapSection = null;
            ushort numReferencedFileSections = binaryReader.ReadUInt16();
            ushort numDataItemSections = binaryReader.ReadUInt16();
            binaryReader.ExpectUInt16(0);

            List<SectionRef<HierarchicalSchemaSection>> hierarchicalSchemaSections = new List<SectionRef<HierarchicalSchemaSection>>(numHierarchicalSchemaSections);

            for (int i = 0; i < numHierarchicalSchemaSections; i++)
                hierarchicalSchemaSections.Add(new SectionRef<HierarchicalSchemaSection>(binaryReader.ReadUInt16()));

            HierarchicalSchemaSections = hierarchicalSchemaSections;

            List<SectionRef<DecisionInfoSection>> decisionInfoSections = new List<SectionRef<DecisionInfoSection>>(numDecisionInfoSections);

            for (int i = 0; i < numDecisionInfoSections; i++)
                decisionInfoSections.Add(new SectionRef<DecisionInfoSection>(binaryReader.ReadUInt16()));

            DecisionInfoSections = decisionInfoSections;

            List<SectionRef<ResourceMapSection>> resourceMapSections = new List<SectionRef<ResourceMapSection>>(numResourceMapSections);

            for (int i = 0; i < numResourceMapSections; i++)
                resourceMapSections.Add(new SectionRef<ResourceMapSection>(binaryReader.ReadUInt16()));

            ResourceMapSections = resourceMapSections;

            List<SectionRef<ReferencedFileSection>> referencedFileSections = new List<SectionRef<ReferencedFileSection>>(numReferencedFileSections);

            for (int i = 0; i < numReferencedFileSections; i++)
                referencedFileSections.Add(new SectionRef<ReferencedFileSection>(binaryReader.ReadUInt16()));

            ReferencedFileSections = referencedFileSections;

            List<SectionRef<DataItemSection>> dataItemSections = new List<SectionRef<DataItemSection>>(numDataItemSections);

            for (int i = 0; i < numDataItemSections; i++)
                dataItemSections.Add(new SectionRef<DataItemSection>(binaryReader.ReadUInt16()));

            DataItemSections = dataItemSections;

            return true;
        }
    }

    [Flags]
    public enum PriDescriptorFlags : ushort
    {
        AutoMerge = 1,
        IsDeploymentMergeable = 2,
        IsDeploymentMergeResult = 4,
        IsAutomergeMergeResult = 8
    }

    public struct SectionRef<T> where T : Section
    {
        internal int sectionIndex;

        internal SectionRef(int sectionIndex)
        {
            this.sectionIndex = sectionIndex;
        }

        public override string ToString()
        {
            return $"Section {typeof(T).Name} at index {sectionIndex}";
        }
    }
}
