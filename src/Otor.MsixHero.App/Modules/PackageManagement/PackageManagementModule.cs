using Otor.MsixHero.App.Controls.PackageExpert;
using Otor.MsixHero.App.Modules.PackageManagement.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement
{
    public class PackageManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageManagementView, PackageManagementViewModel>(NavigationPaths.PackageManagement);
            containerRegistry.RegisterForNavigation<PackagesSearchView>(NavigationPaths.PackageManagementPaths.Search);
            containerRegistry.RegisterForNavigation<PackagesNoDetailsView>(NavigationPaths.PackageManagementPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<PackagesSingleDetailsView>(NavigationPaths.PackageManagementPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<PackagesManyDetailsView>(NavigationPaths.PackageManagementPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.Master, typeof(PackagesListView));
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.PackageExpert, typeof(PackageExpertControl));
        }
    }
}
