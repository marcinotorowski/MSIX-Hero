using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.EventViewer.Views
{
    public partial class EventViewerSearchView
    {
        public EventViewerSearchView()
        {
            InitializeComponent();
        }

        private void ClearSearchField(object sender, RoutedEventArgs e)
        {
            this.SearchBox.Text = string.Empty;
            this.SearchBox.Focus();
            FocusManager.SetFocusedElement(this, this.SearchBox);
            Keyboard.Focus(this.SearchBox);
        }
    }
}
