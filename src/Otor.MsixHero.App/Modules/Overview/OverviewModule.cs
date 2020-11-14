using Otor.MsixHero.App.Modules.Overview.Constants;
using Otor.MsixHero.App.Modules.Overview.ViewModels;
using Otor.MsixHero.App.Modules.Overview.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Overview
{
    public class OverviewModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OverviewView, OverviewViewModel>(PathNames.Overview);
            containerRegistry.RegisterForNavigation<OverviewSearchView>(OverviewNavigationPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
