using System.Windows;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Modules.Packages.Constants;
using Otor.MsixHero.App.Modules.Volumes.Constants;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Packages.Views
{
    /// <summary>
    /// Interaction logic for PackagesView.
    /// </summary>
    public partial class PackagesView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;

        public PackagesView(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.InitializeComponent();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
            this.regionManager.Regions[RegionNames.Search].RequestNavigate(PackagesNavigationPaths.Search);
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
