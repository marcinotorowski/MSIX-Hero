using Otor.MsixHero.App.Events;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dashboard.Views
{
    public partial class DashboardView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;

        public DashboardView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            InitializeComponent();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(250.0));
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
