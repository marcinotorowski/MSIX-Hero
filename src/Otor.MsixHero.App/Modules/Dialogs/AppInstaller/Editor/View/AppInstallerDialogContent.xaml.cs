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

using System.Linq;
using System.Windows.Controls;
using Otor.MsixHero.App.Modules.Dialogs.AppInstaller.Editor.ViewModel;
using Xceed.Wpf.Toolkit;
using YamlDotNet.Core.Tokens;

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

            if (spinner.Content is string contentString)
            {
                if (!int.TryParse(contentString, out var value))
                {
                    return;
                }

                if (e.Direction == SpinDirection.Increase)
                    value++;
                else
                    value--;

                spinner.Content = value.ToString();
            }
            else if (spinner.Content is int contentInt)
            {
                if (e.Direction == SpinDirection.Increase)
                    contentInt++;
                else
                    contentInt--;

                spinner.Content = contentInt;
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = (TabControl)sender;
            var lastTab = tabControl.Items.OfType<TabItem>().Last();
            var selectedTab = tabControl.SelectedItem as TabItem;
            if (lastTab == selectedTab)
            {
#pragma warning disable CS4014
                ((AppInstallerViewModel)this.DataContext).CalculatePadding();
#pragma warning restore CS4014
            }
        }
    }
}
