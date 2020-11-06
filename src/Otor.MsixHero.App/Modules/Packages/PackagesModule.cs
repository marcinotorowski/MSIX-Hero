using Otor.MsixHero.App.Modules.Packages.List.Views;
using Otor.MsixHero.App.Modules.Packages.Page.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Packages
{
    public class PackagesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackagesPageView>(PathNames.PackagesPage);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(RegionNames.Main, typeof(PackagesPageView));
            regionManager.RegisterViewWithRegion("package-list", typeof(PackageListView));
        }
    }
}
