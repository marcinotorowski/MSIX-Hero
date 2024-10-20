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
using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class PackagesConfiguration : BaseJsonSetting
    {
        public PackagesConfiguration()
        {
            this.Sidebar = new SidebarListConfiguration();
            this.Filter = new PackagesFilterConfiguration();
            this.Group = new PackagesGroupConfiguration();
            this.Sorting = new PackagesSortConfiguration();
        }
        
        [DataMember(Name = "filter")]
        public PackagesFilterConfiguration Filter { get; set; }
        
        [DataMember(Name = "group")]
        public PackagesGroupConfiguration Group { get; set; }

        [DataMember(Name = "sorting")]
        public PackagesSortConfiguration Sorting { get; set; }

        [DataMember(Name = "sidebar")]
        public SidebarListConfiguration Sidebar { get; set; }

        [DataMember(Name = "tools")]
        public List<ToolListConfiguration> Tools { get; set; }

        [DataMember(Name = "customPackageDirs")]
        public List<PackageDirectoryConfiguration> CustomPackageDirectories { get; set; }

        [DataMember(Name = "starredApps")]
        public List<string> StarredApps { get; set; }
    }
}