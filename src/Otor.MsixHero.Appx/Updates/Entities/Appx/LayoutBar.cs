using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    public class LayoutBar
    {
        public long Position { get; set; }
        
        public long Size { get; set; }
        
        public ComparisonStatus Status { get; set; }
    }
}