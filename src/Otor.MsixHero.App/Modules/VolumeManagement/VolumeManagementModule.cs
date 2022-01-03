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

using Otor.MsixHero.App.Modules.VolumeManagement.ViewModels;
using Otor.MsixHero.App.Modules.VolumeManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement
{
    public class VolumeManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VolumeManagementView, VolumeManagementViewModel>(NavigationPaths.VolumeManagement);
            containerRegistry.RegisterForNavigation<VolumesSearchView, VolumesSearchViewModel>(NavigationPaths.VolumeManagementPaths.Search);
            containerRegistry.RegisterForNavigation<VolumesNoDetailsView>(NavigationPaths.VolumeManagementPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<VolumesSingleDetailsView, VolumesSingleDetailsViewModel>(NavigationPaths.VolumeManagementPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<VolumesManyDetailsView>(NavigationPaths.VolumeManagementPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(VolumeManagementRegionNames.Master, typeof(VolumesListView));
            ViewModelLocationProvider.Register<VolumesListView, VolumesListViewModel>();
        }
    }
}
