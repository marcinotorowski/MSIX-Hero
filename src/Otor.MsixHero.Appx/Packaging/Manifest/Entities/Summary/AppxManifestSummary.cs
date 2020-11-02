﻿using System;
using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary
{
    [Serializable]
    public class AppxManifestSummary
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string ProcessorArchitecture { get; set; }

        public string Publisher { get; set; }

        public string Logo { get; set; }

        public bool IsFramework { get; set; }

        public string DisplayName { get; set; }
        
        public MsixPackageType PackageType { get; set; }

        public string DisplayPublisher { get; set; }

        public string Description { get; set; }

        public string AccentColor { get; set; }

        // public List<OperatingSystemDependency> OperatingSystemDependencies { get; set; }

        // public List<PackageDependency> PackageDependencies { get; set; }

        public Dictionary<string, string> BuildMetaData { get; set; }
    }
}