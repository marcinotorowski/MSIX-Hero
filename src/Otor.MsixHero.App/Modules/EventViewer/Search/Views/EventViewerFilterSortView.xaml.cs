using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.EventViewer.Search.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels;

namespace Otor.MsixHero.App.Modules.EventViewer.Search.Views
{
    /// <summary>
    /// Interaction logic for EventViewerFilterSortView
    /// </summary>
    public partial class EventViewerFilterSortView
    {
        public EventViewerFilterSortView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var senderRadio = (RadioButton)sender;
            if (senderRadio.IsChecked != true)
            {
                return;
            }

            if (e.Source as RadioButton != senderRadio)
            {
                return;
            }

            var dataContext = (EventViewerFilterSortViewModel) senderRadio.DataContext;
            dataContext.IsDescending = !dataContext.IsDescending;
        }
    }
}
