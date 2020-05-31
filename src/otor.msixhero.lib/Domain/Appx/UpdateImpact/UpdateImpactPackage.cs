using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.UpdateImpact.Blocks;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact
{
    public class UpdateImpactPackage
    {
        public string Path { get; set; }

        public AppxPackage Manifest { get; set; }

        public long? Size { get; set; }

        public IList<Block> Blocks { get; set; }
    }
}