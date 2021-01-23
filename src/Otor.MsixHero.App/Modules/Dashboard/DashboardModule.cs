// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using Otor.MsixHero.App.Modules.Dashboard.ViewModels;
using Otor.MsixHero.App.Modules.Dashboard.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;

namespace Otor.MsixHero.App.Modules.Dashboard
{
    public class DashboardModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>(NavigationPaths.Dashboard);
            containerRegistry.RegisterForNavigation<DashboardSearchView, DashboardSearchViewModel>(NavigationPaths.DashboardPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
