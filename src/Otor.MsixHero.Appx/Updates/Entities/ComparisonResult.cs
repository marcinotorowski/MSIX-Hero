using Otor.MsixHero.Appx.Updates.Entities.Appx;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparisonResult
    {
        public AddedFiles AddedFiles { get; set; }

        public DeletedFiles DeletedFiles { get; set; }

        public ChangedFiles ChangedFiles { get; set; }

        public UnchangedFiles UnchangedFiles { get; set; }

        public ComparedDuplicateFiles DuplicateFiles { get; set; }

        public long UpdateImpact { get; set; }

        public long ActualUpdateImpact { get; set; }

        public long SizeDifference { get; set; }

        public AppxBlockLayout OldPackageLayout { get; set; }
        
        public AppxBlockLayout NewPackageLayout { get; set; }
    }
}