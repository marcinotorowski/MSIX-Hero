using Otor.MsixHero.Ui.Modules.EventViewer.View;
using Otor.MsixHero.Ui.Modules.EventViewer.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.EventViewer
{
    public class EventViewerModule : IModule
    {
        private readonly IRegionManager regionManager;

        public EventViewerModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
            ViewModelLocationProvider.Register<EventViewerView, EventViewerViewModel>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<EventViewerView>(Constants.PathEventViewer);
            containerRegistry.RegisterSingleton(typeof(EventViewerViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            this.regionManager.RegisterViewWithRegion(Constants.RegionContent, typeof(EventViewerView));
        }
    }
}
