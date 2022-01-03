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
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Users;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    public class GetUsersForPackageDto : ProxyObject<List<User>>
    {
        public GetUsersForPackageDto()
        {
        }

        public GetUsersForPackageDto(InstalledPackage package)
        {
            this.Source = package.PackageId;
        }

        public GetUsersForPackageDto(string source)
        {
            this.Source = source;
        }

        [XmlElement]
        public string Source { get; set; }
    }
}
