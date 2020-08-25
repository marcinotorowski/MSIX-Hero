using Otor.MsixHero.Ui.Modules.SystemStatus.View;
using Otor.MsixHero.Ui.Modules.SystemStatus.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.SystemStatus
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
