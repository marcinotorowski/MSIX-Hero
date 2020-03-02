using otor.msixhero.ui.Modules.Settings.View;
using otor.msixhero.ui.Modules.Settings.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Settings
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
