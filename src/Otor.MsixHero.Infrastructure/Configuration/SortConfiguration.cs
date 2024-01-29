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

using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class PackagesSortConfiguration : BaseJsonSetting
    {
        public PackagesSortConfiguration()
        {
            this.SortingMode = PackageSort.Name;
            this.Descending = false;
        }

        [DataMember(Name= "sortingMode")]
        public PackageSort SortingMode { get; set; }

        [DataMember(Name = "descending")]
        public bool Descending { get; set; }
    }
}