using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.Views
{
    /// <summary>
    /// Interaction logic for PackagesSearchView.
    /// </summary>
    public partial class PackagesSearchView
    {
        public PackagesSearchView()
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
