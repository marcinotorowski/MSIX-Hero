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

using System.Windows.Controls;
using Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels;
using Otor.MsixHero.App.Modules.Main.Sidebar.Views;
using Otor.MsixHero.App.Modules.Main.Toolbar.ViewModels;
using Otor.MsixHero.App.Modules.Main.Toolbar.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Main
{
    public class MainModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<Control>(NavigationPaths.Empty);
        }
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.Sidebar, typeof(SidebarView));
            regionManager.RegisterViewWithRegion(RegionNames.Toolbar, typeof(ToolbarView));

            ViewModelLocationProvider.Register<SidebarView, SidebarViewModel>();
            ViewModelLocationProvider.Register<ToolbarView, ToolbarViewModel>();
        }
    }
}
