using Otor.MsixHero.Ui.Modules.PackageList.View;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel;
using Otor.MsixHero.Ui.Modules.PackageList.ViewModel.Elements;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.PackageList
{
    public class PackageListModule : IModule
    {
        private readonly IRegionManager regionManager;

        public PackageListModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ViewModelLocationProvider.Register<PackageListView, PackageListViewModel>();
            ViewModelLocationProvider.Register<MultiSelectionView, MultiSelectionViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageListView>(Constants.PathPackagesList);
            containerRegistry.RegisterForNavigation<EmptySelectionView>(Constants.PathPackagesEmptySelection);
            containerRegistry.RegisterForNavigation<MultiSelectionView>(Constants.PathPackagesMultiSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            regionManager.RegisterViewWithRegion(Constants.RegionContent, typeof(PackageListView));
            this.regionManager.RequestNavigate(Constants.RegionContent, Constants.PathPackagesList);
        }
    }
}
