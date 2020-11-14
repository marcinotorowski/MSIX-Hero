using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Modules.Editors.Signing.NewSelfSigned.ViewModel;

namespace Otor.MsixHero.App.Modules.Editors.Signing.NewSelfSigned.View
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
