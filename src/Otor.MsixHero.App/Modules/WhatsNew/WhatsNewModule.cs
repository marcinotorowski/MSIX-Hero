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
