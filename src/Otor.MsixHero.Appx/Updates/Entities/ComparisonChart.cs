using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class ComparisonChart
    {
        public IList<ComparedChart> Files { get; set; }
        
        public IList<ComparedChart> Blocks { get; set; }
    }
}