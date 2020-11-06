using Otor.MsixHero.App.Events;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.Main.Toolbar.Views
{
    /// <summary>
    /// Interaction logic for ToolbarView.
    /// </summary>
    public partial class ToolbarView
    {
        private readonly IEventAggregator eventAggregator;

        public ToolbarView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            InitializeComponent();
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Subscribe(this.OnTopSearchWidthChangeEvent, ThreadOption.UIThread);
        }

        private void OnTopSearchWidthChangeEvent(TopSearchWidthChangeEventPayLoad obj)
        {
            this.Region.Width = obj.PanelWidth;
        }
    }
}
