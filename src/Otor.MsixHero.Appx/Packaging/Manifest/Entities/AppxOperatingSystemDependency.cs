using System;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxOperatingSystemDependency
    {
        public AppxTargetOperatingSystem Minimum { get; set; }

        public AppxTargetOperatingSystem Tested { get; set; }
    }
}