using System;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary
{
    [Serializable]
    public class AppxManifestSummary
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string ProcessorArchitecture { get; set; }

        public string Publisher { get; set; }

        public string Logo { get; set; }

        public string DisplayName { get; set; }
        
        public PackageType PackageType { get; set; }

        public string DisplayPublisher { get; set; }

        public string Description { get; set; }

        public string AccentColor { get; set; }

        // public List<OperatingSystemDependency> OperatingSystemDependencies { get; set; }

        // public List<PackageDependency> PackageDependencies { get; set; }
    }
}
