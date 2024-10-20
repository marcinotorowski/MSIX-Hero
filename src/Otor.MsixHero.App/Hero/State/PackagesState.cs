// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.Collections.Generic;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.State
{
    public class PackagesState
    {
        public PackagesState()
        {
            this.AllPackages = new List<PackageEntry>();
            this.SelectedPackages = new List<PackageEntry>();
            this.Mode = PackageQuerySource.InstalledForCurrentUser();
        }

        public List<PackageEntry> AllPackages { get; }

        public List<PackageEntry> SelectedPackages { get; }

        public PackageFilter Filter { get; set; }
        
        public string SearchKey { get; set; }

        public PackageQuerySource Mode { get; set; }

        public PackageSort SortMode { get; set; }

        public PackageGroup GroupMode { get; set; }

        public bool SortDescending { get; set; }

        public bool ShowSidebar { get; set; }

        public IReadOnlyList<string> ActivePackageNames { get; set; }

        public AppxPackageArchitecture Platform { get; set; }
    }
}