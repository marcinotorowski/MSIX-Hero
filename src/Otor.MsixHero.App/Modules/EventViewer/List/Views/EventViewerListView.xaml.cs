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

using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.EventViewer.List.Views
{
    /// <summary>
    /// Interaction logic for EventViewerListView.
    /// </summary>
    public partial class EventViewerListView
    {
        private readonly IMsixHeroApplication application;
        
        public EventViewerListView(IMsixHeroApplication application)
        {
            this.application = application;
            this.InitializeComponent();

            this.application.EventAggregator.GetEvent<UiFailedEvent<GetLogsCommand>>().Subscribe(this.OnGetLogsCommandFailed, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutingEvent<GetLogsCommand>>().Subscribe(this.OnGetLogsCommandExecuting);
            this.application.EventAggregator.GetEvent<UiCancelledEvent<GetLogsCommand>>().Subscribe(this.OnGetLogsCommandCancelled, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetLogsCommand>>().Subscribe(this.OnGetLogsCommandExecuted, ThreadOption.UIThread);
            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }
        private void OnGetLogsCommandExecuting(UiExecutingPayload<GetLogsCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetLogsCommandFailed(UiFailedPayload<GetLogsCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetLogsCommandCancelled(UiCancelledPayload<GetLogsCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetLogsCommandExecuted(UiExecutedPayload<GetLogsCommand> obj)
        {
            this.ListBox.SelectedItem = this.ListBox.Items.OfType<LogViewModel>()
                .FirstOrDefault(item => this.application.ApplicationState.EventViewer.SelectedLog == item.Model);

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.application.CommandExecutor.Invoke(this, new SelectLogCommand((this.ListBox.SelectedItem as LogViewModel)?.Model));
        }
        
        private void ListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged -= this.OnSelectionChanged;
            }
        }

        private void ListBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged += this.OnSelectionChanged;

                this.application.CommandExecutor.Invoke(this, new SelectLogCommand((this.ListBox.SelectedItem as LogViewModel)?.Model));
            }
        }
    }
}
