using Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.View;
using Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.AppAttach
{
    public class AppAttachModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<AppAttachView, AppAttachViewModel>(NavigationPaths.DialogPaths.AppAttachEditor);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
