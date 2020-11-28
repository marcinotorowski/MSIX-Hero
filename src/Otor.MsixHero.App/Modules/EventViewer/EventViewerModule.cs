using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Details.Views;
using Otor.MsixHero.App.Modules.EventViewer.List.Views;
using Otor.MsixHero.App.Modules.EventViewer.ViewModels;
using Otor.MsixHero.App.Modules.EventViewer.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer
{
    public class EventViewerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<EventViewerSearchView, EventViewerSearchViewModel>(NavigationPaths.EventViewerPaths.Search);
            containerRegistry.RegisterForNavigation<EventViewerView, EventViewerViewModel>(NavigationPaths.EventViewer);
            containerRegistry.RegisterForNavigation<EventViewerDetailsView, EventViewerDetailsViewModel>(NavigationPaths.EventViewerPaths.Details);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(EventViewerRegionNames.List, typeof(EventViewerListView));
        }
    }
}
