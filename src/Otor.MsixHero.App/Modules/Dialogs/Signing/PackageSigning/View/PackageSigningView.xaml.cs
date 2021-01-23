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

using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.View
{
    /// <summary>
    /// Interaction logic for PackageSigningView.
    /// </summary>
    public partial class PackageSigningView
    {
        public PackageSigningView()
        {
            this.InitializeComponent();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = ((PackageSigningViewModel)this.DataContext).SelectedPackages;
                var list = ((PackageSigningViewModel)this.DataContext).Files;

                for (var i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = selectedItems[i];
                    list.Remove(item);
                }
            }
            else if (e.Command == ApplicationCommands.New)
            {
                ((IDialogAware)this.DataContext).OnDialogOpened(new DialogParameters());
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = ((PackageSigningViewModel)this.DataContext).SelectedPackages;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                e.CanExecute = selectedItems != null && selectedItems.Count != 0;
            }
            else if (e.Command == ApplicationCommands.New)
            {
                e.CanExecute = true;
            }
        }
    }
}
