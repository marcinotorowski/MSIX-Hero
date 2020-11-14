using Otor.MsixHero.App.Modules.Editors.Dependencies.Graph.View;
using Otor.MsixHero.App.Modules.Editors.Dependencies.Graph.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Editors.Dependencies
{
    public class DependenciesModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<DependencyViewerView, DependencyViewerViewModel>(DialogPathNames.DependenciesGraph);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
