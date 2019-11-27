﻿using System;
using System.Collections.Generic;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Full
{
    [Serializable]
    public class AppxPackage
    {
        public string Name { get; set; }

        public string FullName { get; set; }

        public string Path { get; set; }

        public string Publisher { get; set; }

        public string PublisherId { get; set; }

        public string ResourceId { get; set; }

        public string FamilyName { get; set; }

        public string ApplicationUserModelId { get; set; }

        public string Logo { get; set; }

        public string PublisherDisplayName { get; set; }

        public string Description { get; set; }

        public string DisplayName { get; set; }

        public bool IsFramework { get; set; }

        public string Version { get; set; }

        public AppxPackageArchitecture ProcessorArchitecture { get; set; }
        
        public List<AppxPackageDependency> PackageDependencies { get; set; }
        
        public List<AppxOperatingSystemDependency> OperatingSystemDependencies { get; set; }

        public List<AppxApplication> Applications { get; set; }
    }
}
