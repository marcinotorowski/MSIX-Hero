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
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumeManagementViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly PrismServices prismServices;

        public VolumeManagementViewModel(
            IMsixHeroApplication application,
            PrismServices prismServices)
        {
            this.application = application;
            this.prismServices = prismServices;
            application.EventAggregator.GetEvent<UiExecutedEvent<SelectVolumesCommand>>().Subscribe(this.OnSelectVolumes, ThreadOption.UIThread);
        }

        private void OnSelectVolumes(UiExecutedPayload<SelectVolumesCommand> obj)
        {
            var parameters = new NavigationParameters
            {
                { "volumes", this.application.ApplicationState.Volumes.SelectedVolumes }
            };

            switch (this.application.ApplicationState.Volumes.SelectedVolumes.Count)
            {
                case 0:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.ZeroSelection, UriKind.Relative), parameters);
                    break;
                case 1:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.SingleSelection, UriKind.Relative), parameters);
                    break;
                default:
                    this.prismServices.RegionManager.Regions[VolumeManagementRegionNames.Details].NavigationService.RequestNavigate(new Uri(NavigationPaths.VolumeManagementPaths.MultipleSelection, UriKind.Relative), parameters);
                    break;
            }
        }
    }
}
