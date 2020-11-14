using Otor.MsixHero.App.Modules.Editors.Updates.UpdateImpact.View;
using Otor.MsixHero.App.Modules.Editors.Updates.UpdateImpact.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Updates
{
    public class UpdatesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<UpdateImpactView, UpdateImpactViewModel>(DialogPathNames.UpdatesUpdateImpact);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
