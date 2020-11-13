using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.Overview.Views
{
    /// <summary>
    /// Interaction logic for OverviewSearchView.
    /// </summary>
    public partial class OverviewSearchView
    {
        public OverviewSearchView()
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

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.SearchBox.Focus();
            Keyboard.Focus(this.SearchBox);
        }
    }
}
