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

using System.Threading.Tasks;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
{
    public class PackageDriveViewModel : NotifyPropertyChanged
    {
        private readonly IAppxVolumeManager _volumeManagerFactory;

        public PackageDriveViewModel(IAppxVolumeManager volumeManagerFactory)
        {
            this._volumeManagerFactory = volumeManagerFactory;
        }

        public async Task Load(string filePath)
        {
            var disk = await this.GetDisk(filePath).ConfigureAwait(true);
            this.Disk = disk;
            this.OnPropertyChanged(nameof(this.Disk));
        }

        private async Task<AppxVolume> GetDisk(string filePath)
        {
            var mgr = this._volumeManagerFactory;
            var disk = await mgr.GetVolumeForPath(filePath).ConfigureAwait(false);
            if (disk == null)
            {
                return null;
            }

            return disk;
        }

        public AppxVolume Disk { get; private set; }
    }
}
