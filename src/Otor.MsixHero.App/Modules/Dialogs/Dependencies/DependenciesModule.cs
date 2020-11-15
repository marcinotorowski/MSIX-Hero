using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.View;
using Otor.MsixHero.App.Modules.Dialogs.Dependencies.Graph.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Dependencies
{
    public class DependenciesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DependencyViewerView, DependencyViewerViewModel>(NavigationPaths.DialogPaths.DependenciesGraph);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
