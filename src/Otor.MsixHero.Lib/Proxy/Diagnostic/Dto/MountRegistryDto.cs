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

using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Diagnostic.Dto
{
    [Serializable]
    public class MountRegistryDto : ProxyObject
    {
        public MountRegistryDto()
        {
        }

        public MountRegistryDto(string packageName, string installLocation, bool startRegedit = false)
        {
            this.PackageName = packageName;
            this.InstallLocation = installLocation;
            this.StartRegedit = startRegedit;
        }

        public MountRegistryDto(InstalledPackage package, bool startRegedit = false)
        {
            this.PackageName = package.Name;
            this.InstallLocation = package.InstallLocation;
            this.StartRegedit = startRegedit;
        }

        [XmlElement]
        public string PackageName { get; set; }

        [XmlElement]
        public string InstallLocation { get; set; }

        [XmlElement]
        public bool StartRegedit { get; set; }
    }
}
