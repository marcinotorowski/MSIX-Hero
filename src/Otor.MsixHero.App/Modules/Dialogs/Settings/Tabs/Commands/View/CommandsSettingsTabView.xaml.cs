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
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.ViewModel;
using Prism.Common;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.View
{
    public partial class CommandsSettingsTabView
    {
        private readonly ObservableObject<object> _context;

        public CommandsSettingsTabView()
        {
            this.InitializeComponent();

            this._context = RegionContext.GetObservableContext(this);
            this._context.PropertyChanged += ContextOnPropertyChanged;
        }

        private void ContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var context = (SettingsContext)this._context.Value;

            if (this.DataContext is ISettingsComponent dataContext)
            {
                context.Register(dataContext);
            }
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
