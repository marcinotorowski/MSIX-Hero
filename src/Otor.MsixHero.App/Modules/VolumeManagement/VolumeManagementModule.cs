using Otor.MsixHero.App.Modules.VolumeManagement.ViewModels;
using Otor.MsixHero.App.Modules.VolumeManagement.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement
{
    public class VolumeManagementModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VolumeManagementView, VolumeManagementViewModel>(NavigationPaths.VolumeManagement);
            containerRegistry.RegisterForNavigation<VolumesSearchView, VolumesSearchViewModel>(NavigationPaths.VolumeManagementPaths.Search);
            containerRegistry.RegisterForNavigation<VolumesNoDetailsView>(NavigationPaths.VolumeManagementPaths.ZeroSelection);
            containerRegistry.RegisterForNavigation<VolumesSingleDetailsView, VolumesSingleDetailsViewModel>(NavigationPaths.VolumeManagementPaths.SingleSelection);
            containerRegistry.RegisterForNavigation<VolumesManyDetailsView>(NavigationPaths.VolumeManagementPaths.MultipleSelection);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(VolumeManagementRegionNames.Master, typeof(VolumesListView));
            ViewModelLocationProvider.Register<VolumesListView, VolumesListViewModel>();
        }
    }
}
