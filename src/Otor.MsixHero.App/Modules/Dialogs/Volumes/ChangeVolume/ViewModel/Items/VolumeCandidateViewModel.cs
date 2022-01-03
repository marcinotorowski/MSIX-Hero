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

using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes.Entities;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel.Items
{
    public class VolumeCandidateViewModel : NotifyPropertyChanged
    {
        public VolumeCandidateViewModel(AppxVolume model)
        {
            this.Model = model;
        }

        public AppxVolume Model { get; }

        public string PackageStorePath => this.Model.PackageStorePath;

        public string Name => this.Model.Name;
        
        public long SizeTaken => this.Model.Capacity - this.Model.AvailableFreeSpace;

        public long TotalSize => this.Model.Capacity;

        public string DiskLabel => this.Model.DiskLabel;

        public bool IsOffline => this.Model.IsOffline;
    }
}
