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

using System.Collections;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes.Entities;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesSingleDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private AppxVolume volume;

        public AppxVolume Volume
        {
            get => this.volume;
            set => this.SetField(ref this.volume, value);
        }

        void IRegionAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.First().Value is IEnumerable packages)
            {
                this.Volume = packages.OfType<AppxVolume>().FirstOrDefault();
            }
            else if (navigationContext.Parameters.First().Value is AppxVolume singleVolume)
            {
                this.Volume = singleVolume;
            }
            else
            {
                this.Volume = null;
            }
        }

        bool IRegionAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void IRegionAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
