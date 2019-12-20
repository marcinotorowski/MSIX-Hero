using System.Windows;
using System.Windows.Controls;
using otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.Common.CertificateSelector.View
{
    /// <summary>
    /// Interaction logic for PackageSigningView.
    /// </summary>
    public partial class CertificateSelectorView
    {
        public CertificateSelectorView()
        {
            this.InitializeComponent();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((CertificateSelectorViewModel)this.DataContext).Password.CurrentValue = ((PasswordBox)sender).SecurePassword;
        }
    }
}
