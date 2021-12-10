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
using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.AppAttach.Editor.View
{
    /// <summary>
    /// Interaction logic for App Attach View.
    /// </summary>
    public partial class AppAttachView
    {
        public AppAttachView()
        {
            this.InitializeComponent();
        }
        
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var pathOutput = ((AppAttachViewModel)this.DataContext).OutputDirectory;
            Process.Start("explorer.exe", "/select," + pathOutput);
        }

        private async void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = ((AppAttachViewModel)this.DataContext).SelectedPackages;
                if (!selectedItems.Any())
                {
                    return;
                }

                var list = ((AppAttachViewModel)this.DataContext).Files;
                var firstSelectedIndex = selectedItems.Min(p => selectedItems.IndexOf(p));

                for (var i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = selectedItems[i];
                    list.Remove(item);
                }

                if (!list.Any())
                {
                    return;
                }

                if (firstSelectedIndex > 0 && firstSelectedIndex >= list.Count)
                {
                    firstSelectedIndex--;
                }

                selectedItems.Add(list[firstSelectedIndex]);
            }
            else if (e.Command == ApplicationCommands.New)
            {
                ((AppAttachViewModel)this.DataContext).AddPackage();
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                await ((AppAttachViewModel)this.DataContext).ImportFolder().ConfigureAwait(false);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = ((AppAttachViewModel)this.DataContext).SelectedPackages;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                e.CanExecute = selectedItems != null && selectedItems.Count != 0;
            }
            else if (e.Command == ApplicationCommands.New || e.Command == ApplicationCommands.Open)
            {
                e.CanExecute = true;
            }
        }
    }
}
