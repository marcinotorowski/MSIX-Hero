using System;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest
{
    [Serializable]
    public class OperatingSystemDependency
    {
        public TargetOperatingSystem Minimum { get; set; }

        public TargetOperatingSystem Tested { get; set; }
    }
}