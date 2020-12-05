using Otor.MsixHero.App.Modules.WhatsNew.ViewModels;
using Otor.MsixHero.App.Modules.WhatsNew.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.WhatsNew
{
    public class WhatsNewModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WhatsNewView, WhatsNewViewModel>(NavigationPaths.WhatsNew);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
