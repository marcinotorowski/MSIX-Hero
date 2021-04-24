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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.ViewModel;
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
        
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sp = ((AppAttachViewModel)this.DataContext).SelectedPackages;
            sp.Clear();
            sp.AddRange(((ListBox)sender).SelectedItems.OfType<string>());
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);

            var hasData = e.Data.GetDataPresent("FileDrop");
            if (!hasData)
            {
                return;
            }

            var data = e.Data.GetData("FileDrop") as string[];
            if (data == null || !data.Any())
            {
                return;
            }

            var files = ((AppAttachViewModel)this.DataContext).Files;
            foreach (var item in data)
            {
                if (files.Contains(item))
                {
                    continue;
                }

                files.Add(item);
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);

            var hasData = e.Data.GetDataPresent("FileDrop");
            if (!hasData)
            {
                return;
            }

            var data = e.Data.GetData("FileDrop") as string[];
            if (data == null || !data.Any())
            {
                return;
            }

            DropFileObject.SetIsDragging((DependencyObject)sender, true);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);
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
