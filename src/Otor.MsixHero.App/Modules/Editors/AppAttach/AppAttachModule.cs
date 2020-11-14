using Otor.MsixHero.App.Modules.Editors.AppAttach.Editor.View;
using Otor.MsixHero.App.Modules.Editors.AppAttach.Editor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.AppAttach
{
    public class AppAttachModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AppAttachView, AppAttachViewModel>(DialogPathNames.AppAttachEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
