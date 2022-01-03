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
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.Lib.Proxy.Volumes.Dto
{
    public class MovePackageToVolumeDto : ProxyObject
    {
        public MovePackageToVolumeDto()
        {
        }

        public MovePackageToVolumeDto(string volumePath, string packageFullName)
        {
            PackageFullName = packageFullName;
            VolumePackagePath = volumePath;
        }

        public MovePackageToVolumeDto(AppxPackage package, AppxVolume volume)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (volume == null)
            {
                throw new ArgumentNullException(nameof(volume));
            }

            this.PackageFullName = package.FullName;
            this.VolumePackagePath = volume.PackageStorePath;
        }

        public string PackageFullName { get; set; }

        public string VolumePackagePath { get; set; }
    }
}
