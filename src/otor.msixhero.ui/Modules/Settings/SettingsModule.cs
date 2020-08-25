using Otor.MsixHero.Ui.Modules.Settings.View;
using Otor.MsixHero.Ui.Modules.Settings.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.Ui.Modules.Settings
{
    public class SettingsModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<SettingsView, SettingsViewModel>(Constants.PathSettings);
            containerRegistry.Register(typeof(SettingsViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
