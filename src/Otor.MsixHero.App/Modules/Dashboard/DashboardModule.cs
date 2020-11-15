using Otor.MsixHero.App.Modules.Dashboard.ViewModels;
using Otor.MsixHero.App.Modules.Dashboard.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dashboard
{
    public class DashboardModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DashboardView, DashboardViewModel>(NavigationPaths.Dashboard);
            containerRegistry.RegisterForNavigation<DashboardSearchView>(NavigationPaths.DashboardPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
