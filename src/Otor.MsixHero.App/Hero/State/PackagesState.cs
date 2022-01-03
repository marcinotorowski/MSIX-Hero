// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.State
{
    public class PackagesState
    {
        public PackagesState()
        {
            this.AllPackages = new List<InstalledPackage>();
            this.SelectedPackages = new List<InstalledPackage>();
            this.Mode = PackageContext.CurrentUser;
        }

        public List<InstalledPackage> AllPackages { get; }

        public List<InstalledPackage> SelectedPackages { get; }

        public PackageFilter Filter { get; set; }
        
        public string SearchKey { get; set; }

        public PackageContext Mode { get; set; }

        public PackageSort SortMode { get; set; }

        public PackageGroup GroupMode { get; set; }

        public bool SortDescending { get; set; }

        public bool ShowSidebar { get; set; }

        public IReadOnlyList<string> ActivePackageNames { get; set; }

        public AppxPackageArchitecture Platform { get; set; }
    }
}