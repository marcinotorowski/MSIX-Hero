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
using Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for NewSelfSignedView.
    /// </summary>
    public partial class NewSelfSignedView
    {
        public NewSelfSignedView()
        {
            this.InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", ((NewSelfSignedViewModel)this.DataContext).OutputPath.CurrentValue);
        }

        private void HyperlinkImportWizard_OnClick(object sender, RoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).ImportNewCertificate.Execute(null);
        }
    }
}
