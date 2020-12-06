using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.View;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller
{
    public class AppInstallerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<AppInstallerView, AppInstallerViewModel>(NavigationPaths.DialogPaths.AppInstallerEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
