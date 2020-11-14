using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Controls.CertificateSelector.ViewModel;

namespace Otor.MsixHero.App.Controls.CertificateSelector.View
{
    /// <summary>
    /// Interaction logic for PackageSigningView.
    /// </summary>
    public partial class CertificateSelectorView
    {
        public CertificateSelectorView()
        {
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is CertificateSelectorViewModel newDataContext) || newDataContext.Password?.CurrentValue == null)
            {
                return;
            }

            this.PasswordBox.PasswordChanged -= this.PasswordBox_OnPasswordChanged;
            try
            {
                var valuePtr = IntPtr.Zero;
                try
                {
                    valuePtr = Marshal.SecureStringToGlobalAllocUnicode(newDataContext.Password.CurrentValue);
                    // ReSharper disable once AssignNullToNotNullAttribute
                    this.PasswordBox.Password = Marshal.PtrToStringUni(valuePtr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
                }
            }
            finally
            {
                this.PasswordBox.PasswordChanged += this.PasswordBox_OnPasswordChanged;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((CertificateSelectorViewModel)this.DataContext).Password.CurrentValue = ((PasswordBox)sender).SecurePassword;
        }

        private void DeviceGuardMoreInfoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo("https://msixhero.net/redirect/device-guard-signing")
                    {UseShellExecute = true};
                Process.Start(psi);
            }
            catch (Exception)
            {
                MessageBox.Show("Could not open the link:\r\nhttps://msixhero.net/redirect/device-guard-signing");
            }
        }
    }
}
