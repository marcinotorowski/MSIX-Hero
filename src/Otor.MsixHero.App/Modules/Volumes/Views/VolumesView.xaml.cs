using System.Windows;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Modules.Packages.Constants;
using Otor.MsixHero.App.Modules.Volumes.Constants;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Volumes.Views
{
    /// <summary>
    /// Interaction logic for VolumesView.
    /// </summary>
    public partial class VolumesView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;

        public VolumesView(IEventAggregator eventAggregator, IRegionManager regionManager)
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
