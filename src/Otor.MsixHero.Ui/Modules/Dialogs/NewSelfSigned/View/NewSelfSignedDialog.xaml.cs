using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for New Self Signed Dialog.
    /// </summary>
    public partial class NewSelfSignedDialog
    {
        public NewSelfSignedDialog()
        {
            InitializeComponent();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).Password.CurrentValue = ((PasswordBox)sender).Password;
        }
    }
}
