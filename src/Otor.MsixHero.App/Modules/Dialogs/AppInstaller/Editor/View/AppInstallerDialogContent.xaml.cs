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

using Xceed.Wpf.Toolkit;

namespace Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.View
{
    public partial class AppInstallerDialogContent
    {
        public AppInstallerDialogContent()
        {
            InitializeComponent();
        }

        private void Spinner_OnSpin(object sender, SpinEventArgs e)
        {
            var spinner = (ButtonSpinner)sender;
            var content = spinner.Content as string;

            int.TryParse(content ?? "0", out var value);
            if (e.Direction == SpinDirection.Increase)
                value++;
            else
                value--;

            spinner.Content = value.ToString();
        }
    }
}
