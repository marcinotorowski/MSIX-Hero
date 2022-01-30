// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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
                MessageBox.Show(MsixHero.App.Resources.Localization.CertificateSelector_Errors_LinkOpen + System.Environment.NewLine + "https://msixhero.net/redirect/device-guard-signing");
            }
        }

        private async void ComboBoxOnDropDownOpened(object sender, EventArgs e)
        {
            if (!((ComboBox)sender).HasItems)
            {
                ((ComboBox)sender).IsDropDownOpen = false;
                var dc = (CertificateSelectorViewModel)this.DataContext;
                await dc.TimeStampServers.Load(dc.GenerateTimeStampServers()).ConfigureAwait(true);
                ((ComboBox)sender).IsDropDownOpen = true;
            }
        }
    }
}
