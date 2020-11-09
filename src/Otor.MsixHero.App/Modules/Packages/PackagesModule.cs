using Otor.MsixHero.App.Controls.PackageExpert;
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
            containerRegistry.RegisterForNavigation<PackagesNoDetailsView>(PackagesNavigationPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<PackagesSingleDetailsView>(PackagesNavigationPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<PackagesManyDetailsView>(PackagesNavigationPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            // regionManager.RegisterViewWithRegion(RegionNames.Main, typeof(PackagesView));
            regionManager.RegisterViewWithRegion(PackagesRegionNames.Master, typeof(PackagesListView));
            regionManager.RegisterViewWithRegion(PackagesRegionNames.Details, typeof(PackagesNoDetailsView));
            regionManager.RegisterViewWithRegion(PackagesRegionNames.PackageExpert, typeof(PackageExpertControl));
            // regionManager.RegisterViewWithRegion(RegionNames.Search, typeof(PackagesSearchView));
        }
    }
}
