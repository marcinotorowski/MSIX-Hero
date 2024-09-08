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

using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Details.Views;
using Otor.MsixHero.App.Modules.EventViewer.List.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.List.Views;
using Otor.MsixHero.App.Modules.EventViewer.Search.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Search.Views;
using Otor.MsixHero.App.Modules.EventViewer.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer
{
    public class EventViewerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<EventViewerSearchView, EventViewerSearchViewModel>(NavigationPaths.EventViewerPaths.Search);
            containerRegistry.RegisterForNavigation<EventViewerView, EventViewerViewModel>(NavigationPaths.EventViewer);
            containerRegistry.RegisterForNavigation<EventViewerDetailsView, EventViewerDetailsViewModel>(NavigationPaths.EventViewerPaths.Details);
            containerRegistry.RegisterForNavigation<EventViewerNoDetailsView>(NavigationPaths.EventViewerPaths.NoDetails);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(EventViewerRegionNames.PopupFilter, typeof(EventViewerFilterSortView));
            regionManager.RegisterViewWithRegion(EventViewerRegionNames.List, typeof(EventViewerListView));
            ViewModelLocationProvider.Register<EventViewerListView, EventViewerListViewModel>();
            ViewModelLocationProvider.Register<EventViewerFilterSortView, EventViewerFilterSortViewModel>();
        }
    }
}
