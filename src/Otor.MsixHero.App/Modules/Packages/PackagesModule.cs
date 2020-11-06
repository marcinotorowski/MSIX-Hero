using Otor.MsixHero.App.Modules.Packages.Constants;
using Otor.MsixHero.App.Modules.Packages.ViewModels;
using Otor.MsixHero.App.Modules.Packages.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Packages
{
    public class PackagesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackagesView, PackagesViewModel>(PathNames.Packages);
            containerRegistry.RegisterForNavigation<PackagesSearchView>(PackagesNavigationPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            // regionManager.RegisterViewWithRegion(RegionNames.Main, typeof(PackagesView));
            regionManager.RegisterViewWithRegion(PackagesRegionNames.Master, typeof(PackagesListView));
            regionManager.RegisterViewWithRegion(PackagesRegionNames.Details, typeof(PackagesDetailsView));
            regionManager.RegisterViewWithRegion(RegionNames.Search, typeof(PackagesSearchView));
        }
    }
}
