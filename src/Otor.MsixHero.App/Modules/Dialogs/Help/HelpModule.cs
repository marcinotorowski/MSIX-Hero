using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Help.View;
using Otor.MsixHero.App.Modules.Dialogs.Help.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Help
{
    public class HelpModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<HelpView, HelpViewModel>(NavigationPaths.DialogPaths.About);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
