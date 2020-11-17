using Otor.MsixHero.App.Modules.Dialogs.Settings.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings
{
    public class SettingsModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<SettingsView, SettingsViewModel>(NavigationPaths.DialogPaths.Settings);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
