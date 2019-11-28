using System;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
{
    [Serializable]
    public class AppxOperatingSystemDependency
    {
        public AppxTargetOperatingSystem Minimum { get; set; }

        public AppxTargetOperatingSystem Tested { get; set; }
    }
}