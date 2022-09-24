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

using Otor.MsixHero.App.Modules.Containers.List.Views;
using Otor.MsixHero.App.Modules.Containers.Search.ViewModels;
using Otor.MsixHero.App.Modules.Containers.Search.Views;
using Otor.MsixHero.App.Modules.Containers.ViewModels;
using Otor.MsixHero.App.Modules.Containers.Views;
using Otor.MsixHero.App.Modules.Containers.List.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Otor.MsixHero.App.Modules.Containers.Details.ViewModels;
using Otor.MsixHero.App.Modules.Containers.Details.Views;

namespace Otor.MsixHero.App.Modules.Containers
{
    public class ContainersModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ContainersSearchView, ContainersSearchViewModel>(NavigationPaths.ContainersPaths.Search);
            containerRegistry.RegisterForNavigation<ContainersView, ContainersViewModel>(NavigationPaths.Containers);
            containerRegistry.RegisterForNavigation<ContainersDetailsView, ContainersDetailsViewModel>(NavigationPaths.ContainersPaths.Details);
            containerRegistry.RegisterForNavigation<ContainersNoDetailsView>(NavigationPaths.ContainersPaths.NoDetails);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(ContainersRegionNames.PopupFilter, typeof(ContainersFilterSortView));
            regionManager.RegisterViewWithRegion(ContainersRegionNames.List, typeof(ContainersListView));
            ViewModelLocationProvider.Register<ContainersListView, ContainersListViewModel>();
        }
    }
}
