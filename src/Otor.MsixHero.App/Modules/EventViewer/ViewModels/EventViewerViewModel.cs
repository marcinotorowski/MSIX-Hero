using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.ViewModels
{
    public class EventViewerViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly PrismServices prismServices;

        public EventViewerViewModel(
            IMsixHeroApplication application,
            PrismServices prismServices)
        {
            this.application = application;
            this.prismServices = prismServices;
            application.EventAggregator.GetEvent<UiExecutedEvent<SelectLogCommand>>().Subscribe(this.OnSelectLogCommand);
        }

        private void OnSelectLogCommand(UiExecutedPayload<SelectLogCommand> command)
        {
            var parameters = new NavigationParameters();
            parameters.Add("selection", this.application.ApplicationState.EventViewer.SelectedLog);
            prismServices.RegionManager.Regions[EventViewerRegionNames.Details].RequestNavigate(new Uri(NavigationPaths.EventViewerPaths.Details, UriKind.Relative), parameters);
        }
    }
}

