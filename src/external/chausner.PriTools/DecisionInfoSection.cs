using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PriFormat
{
    public class DecisionInfoSection : Section
    {
        public IReadOnlyList<Decision> Decisions { get; private set; }
        public IReadOnlyList<QualifierSet> QualifierSets { get; private set; }
        public IReadOnlyList<Qualifier> Qualifiers { get; private set; }

        internal const string Identifier = "[mrm_decn_info]\0";

        internal DecisionInfoSection(PriFile priFile) : base(Identifier, priFile)
        {
        }

        protected override bool ParseSectionContent(BinaryReader binaryReader)
        {
            ushort numDistinctQualifiers = binaryReader.ReadUInt16();
            ushort numQualifiers = binaryReader.ReadUInt16();
            ushort numQualifierSets = binaryReader.ReadUInt16();
            ushort numDecisions = binaryReader.ReadUInt16();
            ushort numIndexTableEntries = binaryReader.ReadUInt16();
            ushort totalDataLength = binaryReader.ReadUInt16();

            List<DecisionInfo> decisionInfos = new List<DecisionInfo>(numDecisions);
            for (int i = 0; i < numDecisions; i++)
            {
                ushort firstQualifierSetIndexIndex = binaryReader.ReadUInt16();
                ushort numQualifierSetsInDecision = binaryReader.ReadUInt16();
                decisionInfos.Add(new DecisionInfo(firstQualifierSetIndexIndex, numQualifierSetsInDecision));
            }

            List<QualifierSetInfo> qualifierSetInfos = new List<QualifierSetInfo>(numQualifierSets);
            for (int i = 0; i < numQualifierSets; i++)
            {
                ushort firstQualifierIndexIndex = binaryReader.ReadUInt16();
                ushort numQualifiersInSet = binaryReader.ReadUInt16();
                qualifierSetInfos.Add(new QualifierSetInfo(firstQualifierIndexIndex, numQualifiersInSet));
            }

            List<QualifierInfo> qualifierInfos = new List<QualifierInfo>(numQualifiers);
            for (int i = 0; i < numQualifiers; i++)
            {
                ushort index = binaryReader.ReadUInt16();
                ushort priority = binaryReader.ReadUInt16();
                ushort fallbackScore = binaryReader.ReadUInt16();
                binaryReader.ExpectUInt16(0);
                qualifierInfos.Add(new QualifierInfo(index, priority, fallbackScore));
            }

            List<DistinctQualifierInfo> distinctQualifierInfos = new List<DistinctQualifierInfo>(numDistinctQualifiers);
            for (int i = 0; i < numDistinctQualifiers; i++)
            {
                binaryReader.ReadUInt16();
                QualifierType qualifierType = (QualifierType)binaryReader.ReadUInt16();
                binaryReader.ReadUInt16();
                binaryReader.ReadUInt16();
                uint operandValueOffset = binaryReader.ReadUInt32();
                distinctQualifierInfos.Add(new DistinctQualifierInfo(qualifierType, operandValueOffset));
            }

            ushort[] indexTable = new ushort[numIndexTableEntries];

            for (int i = 0; i < numIndexTableEntries; i++)
                indexTable[i] = binaryReader.ReadUInt16();

            long dataStartOffset = binaryReader.BaseStream.Position;

            List<Qualifier> qualifiers = new List<Qualifier>(numQualifiers);

            for (int i = 0; i < numQualifiers; i++)
            {
                DistinctQualifierInfo distinctQualifierInfo = distinctQualifierInfos[qualifierInfos[i].Index];

                binaryReader.BaseStream.Seek(dataStartOffset + distinctQualifierInfo.OperandValueOffset * 2, SeekOrigin.Begin);                

                string value = binaryReader.ReadNullTerminatedString(Encoding.Unicode);

                qualifiers.Add(new Qualifier(
                    (ushort)i,
                    distinctQualifierInfo.QualifierType,
                    qualifierInfos[i].Priority,
                    qualifierInfos[i].FallbackScore / 1000f,
                    value));
            }

            Qualifiers = qualifiers;

            List<QualifierSet> qualifierSets = new List<QualifierSet>(numQualifierSets);

            for (int i = 0; i < numQualifierSets; i++)
            {
                List<Qualifier> qualifiersInSet = new List<Qualifier>(qualifierSetInfos[i].NumQualifiersInSet);

                for (int j = 0; j < qualifierSetInfos[i].NumQualifiersInSet; j++)
                    qualifiersInSet.Add(qualifiers[indexTable[qualifierSetInfos[i].FirstQualifierIndexIndex + j]]);

                qualifierSets.Add(new QualifierSet((ushort)i, qualifiersInSet));
            }

            QualifierSets = qualifierSets;

            List<Decision> decisions = new List<Decision>(numDecisions);

            for (int i = 0; i < numDecisions; i++)
            {
                List<QualifierSet> qualifierSetsInDecision = new List<QualifierSet>(decisionInfos[i].NumQualifierSetsInDecision);

                for (int j = 0; j < decisionInfos[i].NumQualifierSetsInDecision; j++)
                    qualifierSetsInDecision.Add(qualifierSets[indexTable[decisionInfos[i].FirstQualifierSetIndexIndex + j]]);

                decisions.Add(new Decision((ushort)i, qualifierSetsInDecision));
            }

            Decisions = decisions;

            return true;
        }

        private struct DecisionInfo
        {
            public ushort FirstQualifierSetIndexIndex;
            public ushort NumQualifierSetsInDecision;

            public DecisionInfo(ushort firstQualifierSetIndexIndex, ushort numQualifierSetsInDecision)
            {
                FirstQualifierSetIndexIndex = firstQualifierSetIndexIndex;
                NumQualifierSetsInDecision = numQualifierSetsInDecision;
            }
        }

        private struct QualifierSetInfo
        {
            public ushort FirstQualifierIndexIndex;
            public ushort NumQualifiersInSet;

            public QualifierSetInfo(ushort firstQualifierIndexIndex, ushort numQualifiersInSet)
            {
                FirstQualifierIndexIndex = firstQualifierIndexIndex;
                NumQualifiersInSet = numQualifiersInSet;
            }
        }

        private struct QualifierInfo
        {
            public ushort Index;
            public ushort Priority;
            public ushort FallbackScore;

            public QualifierInfo(ushort index, ushort priority, ushort fallbackScore)
            {
                Index = index;
                Priority = priority;
                FallbackScore = fallbackScore;
            }
        }

        private struct DistinctQualifierInfo
        {
            public QualifierType QualifierType;
            public uint OperandValueOffset;

            public DistinctQualifierInfo(QualifierType qualifierType, uint operandValueOffset)
            {
                QualifierType = qualifierType;
                OperandValueOffset = operandValueOffset;
            }
        }
    }

    public enum QualifierType
    {
        Language,
        Contrast,
        Scale,
        HomeRegion,
        TargetSize,
        LayoutDirection,
        Theme,
        AlternateForm,
        DXFeatureLevel,
        Configuration,
        DeviceFamily,
        Custom
    }

    public class Qualifier
    {
        public ushort Index { get; }
        public QualifierType Type { get; }
        public ushort Priority { get; }
        public float FallbackScore { get; }
        public string Value { get; }

        internal Qualifier(ushort index, QualifierType type, ushort priority, float fallbackScore, string value)
        {
            Index = index;
            Type = type;
            Priority = priority;
            FallbackScore = fallbackScore;
            Value = value;
        }

        public override string ToString()
        {
            return $"Index: {Index} Type: {Type} Value: {Value} Priority: {Priority} FallbackScore: {FallbackScore}";
        }
    }

    public class QualifierSet
    {
        public ushort Index { get; }
        public IReadOnlyList<Qualifier> Qualifiers { get; }

        internal QualifierSet(ushort index, IReadOnlyList<Qualifier> qualifiers)
        {
            Index = index;
            Qualifiers = qualifiers;
        }

        public override string ToString()
        {
            return $"Index: {Index} Qualifiers: {Qualifiers.Count}";
        }
    }

    public class Decision
    {
        public ushort Index { get; }
        public IReadOnlyList<QualifierSet> QualifierSets { get; }

        internal Decision(ushort index, IReadOnlyList<QualifierSet> qualifierSets)
        {
            Index = index;
            QualifierSets = qualifierSets;
        }

        public override string ToString()
        {
            return $"Index: {Index} Qualifier sets: {QualifierSets.Count}";
        }
    }
}
