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

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tabs.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View.Tabs.Commands
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class CommandsSettingsView
    {
        public CommandsSettingsView()
        {
            this.InitializeComponent();
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            var dataContext = ((SettingsViewModel)this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }

        private void CommandsDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CommandsSettingsTabViewModel dataContext)
            {
                dataContext.Items.CollectionChanged += this.OnCommandsCollectionChanged;
            }
        }

        private void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.ToolDisplayName.Focus();
                FocusManager.SetFocusedElement(this, this.ToolDisplayName);
                Keyboard.Focus(this.ToolDisplayName);
                this.ToolDisplayName.SelectAll();
            }
        }
    }
}
