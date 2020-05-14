using System.Windows;
using otor.msixhero.ui.Modules.Dialogs.ChangeVolume.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.ChangeVolume.View
{
    /// <summary>
    /// Interaction logic for Change Volume View.
    /// </summary>
    public partial class ChangeVolumeView
    {
        public ChangeVolumeView()
        {
            InitializeComponent();
        }

        private void CreateNew(object sender, RoutedEventArgs e)
        {
            ((ChangeVolumeViewModel) this.DataContext).CreateNew();
        }
    }
}
