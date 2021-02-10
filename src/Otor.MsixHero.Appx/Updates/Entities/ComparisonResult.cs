namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparisonResult : BaseCompared
    {
        public ComparedFiles NewFiles { get; set; }

        public ComparedFiles DeletedFiles { get; set; }
        
        public ComparedDuplicateFiles DuplicateFiles { get; set; }

        public ComparedFiles ChangedFiles { get; set; }

        public ComparedFiles UnchangedFiles { get; set; }
        
        public long BaseTotalCompressedSize { get; set; }

        public long TargetTotalCompressedSize { get; set; }

        public ComparisonChart BaseChart { get; set; }

        public ComparisonChart TargetChart { get; set; }
    }
}