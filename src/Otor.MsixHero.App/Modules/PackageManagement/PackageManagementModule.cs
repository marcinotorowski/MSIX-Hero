using Otor.MsixHero.App.Controls.PackageExpert;
using Otor.MsixHero.App.Modules.EventViewer.List.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.List.Views;
using Otor.MsixHero.App.Modules.PackageManagement.Details.Views;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.Views;
using Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Search.Views;
using Otor.MsixHero.App.Modules.PackageManagement.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement
{
    public class PackageManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageManagementView, PackageManagementViewModel>(NavigationPaths.PackageManagement);
            containerRegistry.RegisterForNavigation<PackagesSearchView, PackagesSearchViewModel>(NavigationPaths.PackageManagementPaths.Search);
            containerRegistry.RegisterForNavigation<PackagesNoDetailsView>(NavigationPaths.PackageManagementPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<PackagesSingleDetailsView>(NavigationPaths.PackageManagementPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<PackagesManyDetailsView>(NavigationPaths.PackageManagementPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.Master, typeof(PackagesListView));
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.PackageExpert, typeof(PackageExpertControl));
            regionManager.RegisterViewWithRegion(PackageManagementRegionNames.PopupFilter, typeof(PackageFilterSortView));

            ViewModelLocationProvider.Register<PackagesListView, PackagesListViewModel>();
            ViewModelLocationProvider.Register<PackageFilterSortView, PackageFilterSortViewModel>();
        }
    }
}
