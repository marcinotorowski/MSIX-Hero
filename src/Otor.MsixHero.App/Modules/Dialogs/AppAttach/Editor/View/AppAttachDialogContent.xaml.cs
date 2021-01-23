// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using System.Diagnostics;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.View
{
    public partial class AppAttachDialogContent
    {
        public AppAttachDialogContent()
        {
            InitializeComponent();
        }
        private void HyperlinkMsdn_OnClick(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo("https://msixhero.net/redirect/msix-app-attach/prepare-ps");
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void OnSpin(object sender, SpinEventArgs e)
        {
            var spinner = (ButtonSpinner)sender;
            var content = spinner.Content as string;

            int.TryParse(content ?? "0", out var value);
            if (e.Direction == SpinDirection.Increase)
                value += 10;
            else
                value -= 10;

            spinner.Content = value.ToString();
        }
    }
}
