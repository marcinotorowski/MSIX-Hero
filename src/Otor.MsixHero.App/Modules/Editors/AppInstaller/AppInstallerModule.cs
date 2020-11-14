using Otor.MsixHero.App.Modules.Editors.AppInstaller.Editor.View;
using Otor.MsixHero.App.Modules.Editors.AppInstaller.Editor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.AppInstaller
{
    public class AppInstallerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AppInstallerView, AppInstallerViewModel>(DialogPathNames.AppInstallerEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
