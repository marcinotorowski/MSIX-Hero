using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Common.CertificateSelector.View
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
    }
}
