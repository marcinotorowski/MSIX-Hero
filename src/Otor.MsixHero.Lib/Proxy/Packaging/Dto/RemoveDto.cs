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
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class RemoveDto : ProxyObject
    {
        public RemoveDto()
        {
            this.Packages = new List<InstalledPackage>();
        }

        public RemoveDto(PackageContext context, IEnumerable<InstalledPackage> packages) : this()
        {
            this.Context = context;
            this.Packages.AddRange(packages);
        }

        public RemoveDto(IEnumerable<InstalledPackage> packages) : this(PackageContext.CurrentUser, packages)
        {
        }

        public RemoveDto(PackageContext context, params InstalledPackage[] packages) : this(context, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public RemoveDto(params InstalledPackage[] packages) : this(PackageContext.CurrentUser, (IEnumerable<InstalledPackage>)packages)
        {
        }

        public List<InstalledPackage> Packages { get; set; }

        public PackageContext Context { get; set;  }

        public bool RemoveAppData { get; set; }
    }
}
