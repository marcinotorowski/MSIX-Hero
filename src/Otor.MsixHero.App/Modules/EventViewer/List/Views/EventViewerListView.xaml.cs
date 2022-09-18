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
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
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

            this.application.EventAggregator.GetEvent<UiFailedEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommandFailed, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutingEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommandExecuting);
            this.application.EventAggregator.GetEvent<UiCancelledEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommandCancelled, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommandExecuted, ThreadOption.UIThread);
            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }
        private void OnGetLogsCommandExecuting(UiExecutingPayload<GetEventsCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetLogsCommandFailed(UiFailedPayload<GetEventsCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetLogsCommandCancelled(UiCancelledPayload<GetEventsCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetLogsCommandExecuted(UiExecutedPayload<GetEventsCommand> obj)
        {
            this.ListBox.SelectedItem = this.ListBox.Items.OfType<EventViewModel>()
                .FirstOrDefault(item => this.application.ApplicationState.EventViewer.SelectedAppxEvent == item.Model);

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.application.CommandExecutor.Invoke(this, new SelectEventCommand((this.ListBox.SelectedItem as EventViewModel)?.Model));
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

                this.application.CommandExecutor.Invoke(this, new SelectEventCommand((this.ListBox.SelectedItem as EventViewModel)?.Model));
            }
        }
    }
}
