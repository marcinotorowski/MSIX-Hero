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

using Otor.MsixHero.App.Modules.PackageManagement.Details.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Details.Views;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.Views;
using Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Search.Views;
using Otor.MsixHero.App.Modules.PackageManagement.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement
{
    public class PackageManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageManagementView, PackageManagementViewModel>(NavigationPaths.PackageManagement);
            containerRegistry.RegisterForNavigation<PackagesSearchView, PackagesSearchViewModel>(NavigationPaths.PackageManagementPaths.Search);
            containerRegistry.RegisterForNavigation<PackagesNoDetailsView>(NavigationPaths.PackageManagementPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<PackagesSingleDetailsView>(NavigationPaths.PackageManagementPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<PackagesManyDetailsView, PackageManyDetailsViewModel>(NavigationPaths.PackageManagementPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.Master, typeof(PackagesListView));
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.PackageExpert, typeof(PackageContentHost));
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.PopupFilter, typeof(PackageFilterSortView));

            ViewModelLocationProvider.Register<PackagesListView, PackagesListViewModel>();
            ViewModelLocationProvider.Register<PackageFilterSortView, PackageFilterSortViewModel>();
        }
    }
}
