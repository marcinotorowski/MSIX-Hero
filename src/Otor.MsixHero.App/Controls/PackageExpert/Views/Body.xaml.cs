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
using System.IO;
using System.Windows;
using System.Windows.Documents;
using Notifications.Wpf.Core;
using Otor.MsixHero.App.Services;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views
{
    /// <summary>
    /// Interaction logic for PackageExpert
    /// </summary>
    public partial class Body
    {
        public Body()
        {
            this.InitializeComponent();
        }

        private void HyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            ExceptionGuard.Guard(() =>
            {
                var psi = new ProcessStartInfo((string)((Hyperlink)sender).Tag)
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            }, 
            new InteractionService(new NotificationManager()));
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            ExceptionGuard.Guard(() =>
            {
                var dir = (string)((Hyperlink) sender).Tag;
                Process.Start("explorer.exe", "/select," + Path.Combine(dir, FileConstants.AppxManifestFile));
            }, 
            new InteractionService(new NotificationManager()));
        }
    }
}
