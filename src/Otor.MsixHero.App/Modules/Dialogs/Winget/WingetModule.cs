using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.WinGet.YamlEditor.View;
using Otor.MsixHero.App.Modules.Dialogs.WinGet.YamlEditor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.WinGet
{
    public class WingetModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<WingetView, WinGetViewModel>(NavigationPaths.DialogPaths.WingetYamlEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
