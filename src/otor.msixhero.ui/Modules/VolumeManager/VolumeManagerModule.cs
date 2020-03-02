using otor.msixhero.ui.Modules.VolumeManager.View;
using otor.msixhero.ui.Modules.VolumeManager.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.VolumeManager
{
    public class VolumeManagerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public VolumeManagerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ViewModelLocationProvider.Register<VolumeManagerView, VolumeManagerViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<VolumeManagerView>(Constants.PathVolumeManager);
            containerRegistry.RegisterSingleton(typeof(VolumeManagerViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            this.regionManager.RegisterViewWithRegion(Constants.RegionContent, typeof(VolumeManagerView));
        }
    }
}
