using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Updates.Entities.Appx
{
    public class PackageLayout
    {
        public IList<LayoutBar> Files { get; set; }
        
        public IList<LayoutBar> Blocks { get; set; }
    }
}