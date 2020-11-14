using Otor.MsixHero.App.Modules.Editors.Winget.YamlEditor.View;
using Otor.MsixHero.App.Modules.Editors.Winget.YamlEditor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Winget
{
    public class WingetModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<WingetView, WingetViewModel>(DialogPathNames.WingetYamlEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
