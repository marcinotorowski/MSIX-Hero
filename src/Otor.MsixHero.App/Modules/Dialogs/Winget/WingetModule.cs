using Otor.MsixHero.App.Modules.Dialogs.Winget.YamlEditor.View;
using Otor.MsixHero.App.Modules.Dialogs.Winget.YamlEditor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Winget
{
    public class WingetModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WingetView, WingetViewModel>(NavigationPaths.DialogPaths.WingetYamlEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
