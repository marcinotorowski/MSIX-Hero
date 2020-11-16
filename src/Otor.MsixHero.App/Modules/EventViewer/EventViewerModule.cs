using Otor.MsixHero.App.Modules.EventViewer.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.EventViewer
{
    public class EventViewerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<EventViewerSearchView, EventViewerSearchViewModel>(NavigationPaths.EventViewerPaths.Search);
            containerRegistry.RegisterForNavigation<EventViewerView, EventViewerViewModel>(NavigationPaths.EventViewer);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
