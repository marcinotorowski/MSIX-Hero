using System;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Full
{
    [Serializable]
    public class AppxOperatingSystemDependency
    {
        public AppxTargetOperatingSystem Minimum { get; set; }

        public AppxTargetOperatingSystem Tested { get; set; }
    }
}