using System;
using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Build;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxPackage
    {
        public string RootFolder { get; set; }

        public string Name { get; set; }
        
        public string FullName { get; set; }

        public string Path { get; set; }

        public string Publisher { get; set; }
        
        public string ResourceId { get; set; }

        public string FamilyName { get; set; }

        public string ApplicationUserModelId { get; set; }

        public byte[] Logo { get; set; }

        public string PublisherDisplayName { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public bool IsFramework { get; set; }
        
        public bool IsDevelopment { get; set; }
        
        public bool IsBundle { get; set; }

        public bool IsOptional { get; set; }

        public bool IsResource { get; set; }

        public string Version { get; set; }

        public AppxPackageArchitecture ProcessorArchitecture { get; set; }
        
        public List<AppxPackageDependency> PackageDependencies { get; set; }
        
        public List<AppxOperatingSystemDependency> OperatingSystemDependencies { get; set; }

        public List<AppxApplication> Applications { get; set; }

        public BuildInfo BuildInfo { get; set; }

        public List<AppxCapability> Capabilities { get; set; }

        public bool PackageIntegrity { get; set; }

        public Uri AppInstallerUri { get; set; }
    }
}
