using System.Windows;
using Otor.MsixHero.App.Events;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    public partial class VolumeManagementView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;

        public VolumeManagementView(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
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
