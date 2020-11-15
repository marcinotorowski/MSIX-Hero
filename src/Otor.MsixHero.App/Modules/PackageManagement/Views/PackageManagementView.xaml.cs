using System.Windows;
using Otor.MsixHero.App.Events;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.Views
{
    public partial class PackageManagementView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;

        public PackageManagementView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InitializeComponent();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void RegionOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }
    }
}
