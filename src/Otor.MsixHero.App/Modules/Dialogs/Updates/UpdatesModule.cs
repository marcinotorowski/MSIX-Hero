using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View;
using Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates
{
    public class UpdatesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<UpdateImpactView, UpdateImpactViewModel>(NavigationPaths.DialogPaths.UpdatesUpdateImpact);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
