using Otor.MsixHero.App.Events;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Main.Toolbar.Views
{
    /// <summary>
    /// Interaction logic for ToolbarView.
    /// </summary>
    public partial class ToolbarView
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;

        public ToolbarView(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            InitializeComponent();
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Subscribe(this.OnTopSearchWidthChangeEvent, ThreadOption.UIThread);
        }

        private void OnTopSearchWidthChangeEvent(TopSearchWidthChangeEventPayLoad obj)
        {
            this.Region.Width = obj.PanelWidth;
        }
    }
}
