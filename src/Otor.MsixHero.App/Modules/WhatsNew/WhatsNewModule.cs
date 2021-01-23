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

using System;
using Otor.MsixHero.App.Modules.WhatsNew.ViewModels;
using Otor.MsixHero.App.Modules.WhatsNew.Views;
using Otor.MsixHero.App.Modules.WhatsNew.Views.Tabs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.WhatsNew
{
    public class WhatsNewModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WhatsNewView, WhatsNewViewModel>(NavigationPaths.WhatsNew);
            containerRegistry.RegisterForNavigation<WhatsNew1>("whats-new1");
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(WhatsNewRegionNames.Master, typeof(WhatsNew1));
            regionManager.RegisterViewWithRegion(WhatsNewRegionNames.Master, typeof(WhatsNew2));
            regionManager.RegisterViewWithRegion(WhatsNewRegionNames.Master, typeof(WhatsNew3));
            regionManager.RegisterViewWithRegion(WhatsNewRegionNames.Master, typeof(WhatsNew4));
            regionManager.RequestNavigate(WhatsNewRegionNames.Master, new Uri("whats-new1", UriKind.Relative));
        }
    }
}
