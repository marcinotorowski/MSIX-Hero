using System.Windows;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.EventViewer.ViewModels;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.Views
{
    /// <summary>
    /// Interaction logic for EventViewerView.
    /// </summary>
    public partial class EventViewerView : INavigationAware
    {
        private readonly IMsixHeroApplication application;
        private readonly EventViewerCommandHandler commandHandler;

        public EventViewerView(IMsixHeroApplication application, IInteractionService interactionService, IBusyManager busyManager)
        {
            this.application = application;
            this.InitializeComponent();
            this.commandHandler = new EventViewerCommandHandler(this, application, interactionService, busyManager);
        }

        private void RegionOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
