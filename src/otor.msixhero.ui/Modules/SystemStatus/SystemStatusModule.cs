using otor.msixhero.ui.Modules.SystemStatus.View;
using otor.msixhero.ui.Modules.SystemStatus.ViewModel;
using otor.msixhero.ui.Modules.VolumeManager.View;
using otor.msixhero.ui.Modules.VolumeManager.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.SystemStatus
{
    public class SystemStatusModule : IModule
    {
        private readonly IRegionManager regionManager;

        public SystemStatusModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ViewModelLocationProvider.Register<SystemStatusView, SystemStatusViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SystemStatusView>(Constants.PathSystemStatus);
            containerRegistry.RegisterSingleton(typeof(SystemStatusViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            this.regionManager.RegisterViewWithRegion(Constants.RegionContent, typeof(SystemStatusView));
        }
    }
}
