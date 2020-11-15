using Otor.MsixHero.App.Modules.SystemStatus.View;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.SystemStatus
{
    public class SystemStatusModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<SystemStatusView, SystemStatusViewModel>(NavigationPaths.SystemStatus);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
