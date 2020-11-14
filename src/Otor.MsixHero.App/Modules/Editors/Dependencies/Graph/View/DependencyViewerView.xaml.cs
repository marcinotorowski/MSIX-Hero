using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Otor.MsixHero.App.Modules.Editors.Dependencies.Graph.View
{
    /// <summary>
    /// Interaction logic for DependencyViewer view.
    /// </summary>
    public partial class DependencyViewerView
    {
        public DependencyViewerView()
        {
            this.InitializeComponent();
        }

        private void ToggleButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (((ToggleButton)sender).IsChecked == true)
            {
                e.Handled = true;
            }
        }

        private void BeforeButtonClick(object sender, MouseButtonEventArgs e)
        {
            this.ToggleButton.IsChecked = false;
        }
    }
}
