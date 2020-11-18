using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    /// <summary>
    /// Interaction logic for VolumesSearchView.
    /// </summary>
    public partial class VolumesSearchView
    {
        public VolumesSearchView()
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
