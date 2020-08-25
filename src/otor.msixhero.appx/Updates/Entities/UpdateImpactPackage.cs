using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class UpdateImpactPackage
    {
        public string Path { get; set; }

        public AppxPackage Manifest { get; set; }

        public long? Size { get; set; }

        public IList<Block> Blocks { get; set; }
    }
}