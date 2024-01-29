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

using System;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Volumes.Entities
{
    [Serializable]
    public class AppxVolume
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string PackageStorePath { get; set; }

        [XmlAttribute]
        public bool IsDefault { get; set; }

        [XmlAttribute]
        public ulong Capacity { get; set; }

        [XmlAttribute]
        public string DiskLabel { get; set; }

        [XmlAttribute]
        public ulong AvailableFreeSpace { get; set; }

        public ulong OccupiedSpace => this.Capacity - this.AvailableFreeSpace;
        public int Percent => this.AvailableFreeSpace == 0 ? 0 : (int)(100 * (float)(Capacity - AvailableFreeSpace) / AvailableFreeSpace);

        [XmlAttribute]
        public bool IsDriveReady { get; set; }

        [XmlAttribute]
        public bool IsOffline { get; set; }

        [XmlAttribute]
        public bool IsSystem { get; set; }
    }
}
