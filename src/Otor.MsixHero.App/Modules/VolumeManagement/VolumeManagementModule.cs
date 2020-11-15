using Otor.MsixHero.App.Modules.VolumeManagement.ViewModels;
using Otor.MsixHero.App.Modules.VolumeManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement
{
    public class VolumeManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VolumeManagementView, VolumeManagementViewModel>(NavigationPaths.VolumeManagement);
            containerRegistry.RegisterForNavigation<VolumesSearchView>(NavigationPaths.VolumeManagementPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(VolumeManagementRegionNames.Master, typeof(VolumesListView));
            regionManager.RegisterViewWithRegion(VolumeManagementRegionNames.Details, typeof(VolumesDetailsView));
            regionManager.RegisterViewWithRegion(RegionNames.Search, typeof(VolumesSearchView));
        }
    }
}
