using Otor.MsixHero.App.Modules.Volumes.Constants;
using Otor.MsixHero.App.Modules.Volumes.ViewModels;
using Otor.MsixHero.App.Modules.Volumes.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Volumes
{
    public class VolumesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VolumesView, VolumesViewModel>(PathNames.Volumes);
            containerRegistry.RegisterForNavigation<VolumesSearchView>(VolumesNavigationPaths.Search);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            // regionManager.RegisterViewWithRegion(RegionNames.Main, typeof(VolumesView));
            regionManager.RegisterViewWithRegion(VolumesRegionNames.Master, typeof(VolumesListView));
            regionManager.RegisterViewWithRegion(VolumesRegionNames.Details, typeof(VolumesDetailsView));
            regionManager.RegisterViewWithRegion(RegionNames.Search, typeof(VolumesSearchView));
        }
    }
}
