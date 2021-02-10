using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public class ComparedDuplicateFiles : BaseDuplicateFiles
    {
        public List<ComparedDuplicate> Duplicates { get; set; }

        public long TargetTotalBlockSize { get; set; }

        public long TargetTotalBlockCount { get; set; }

        public long TargetTotalFileSize { get; set; }

        public long TargetTotalFileCount { get; set; }
    }
}