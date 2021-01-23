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
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{

    [Serializable]
    public class CheckIfInstalledDto : ProxyObject<bool>
    {
        public CheckIfInstalledDto()
        {
            this.Context = PackageContext.CurrentUser;
        }

        public CheckIfInstalledDto(PackageContext context)
        {
            this.Context = context;
        }

        [XmlElement]
        public PackageContext Context { get; set; }
        
        [XmlElement]
        public string ManifestFilePath { get; set; }
    }
}
