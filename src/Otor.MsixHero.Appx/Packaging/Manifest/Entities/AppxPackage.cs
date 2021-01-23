// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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

        public List<AppxMainPackageDependency> MainPackages { get; set; }
        
        public List<AppxOperatingSystemDependency> OperatingSystemDependencies { get; set; }

        public List<AppxApplication> Applications { get; set; }

        public BuildInfo BuildInfo { get; set; }

        public List<AppxCapability> Capabilities { get; set; }

        public bool PackageIntegrity { get; set; }

        public AppxSource Source { get; set; }
    }
}
