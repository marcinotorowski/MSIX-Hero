﻿// MSIX Hero
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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.EventViewer.List.ViewModels
{
    public class EventViewerListViewModel : NotifyPropertyChanged, IActiveAware
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;
        private bool firstRun = true;
        private bool isActive;

        public EventViewerListViewModel(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.Logs = new ObservableCollection<LogViewModel>();
            this.LogsView = CollectionViewSource.GetDefaultView(this.Logs);
            this.LogsView.Filter += Filter;
            this.Sort(nameof(Log.DateTime), false);
            this.MaxLogs = 250;
            this.End = DateTime.Now;
            this.Start = this.End.Subtract(TimeSpan.FromDays(5));

            this.busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.Progress = new ProgressProperty();
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetLogsCommand, IList<Log>>>().Subscribe(this.OnGetLogs, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerFilterCommand>>().Subscribe(this.OnSetEventViewerFilterCommand, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerSortingCommand>>().Subscribe(this.OnSetEventViewerSortingCommand, ThreadOption.UIThread);
            this.SetSortingAndGrouping();
        }

        private void SetSortingAndGrouping()
        {
            var currentSort = this.application.ApplicationState.EventViewer.SortMode;
            var currentSortDescending = this.application.ApplicationState.EventViewer.SortDescending;

            using (this.LogsView.DeferRefresh())
            {
                string sortProperty;

                switch (currentSort)
                {
                    case EventSort.Date:
                        sortProperty = nameof(LogViewModel.DateTime);
                        break;
                    case EventSort.Type:
                        sortProperty = nameof(LogViewModel.Level);
                        break;
                    case EventSort.PackageName:
                        sortProperty = nameof(LogViewModel.PackageName);
                        break;
                    default:
                        sortProperty = null;
                        break;
                }

                if (sortProperty == null)
                {
                    this.LogsView.SortDescriptions.Clear();
                }
                else
                {
                    var sd = this.LogsView.SortDescriptions.FirstOrDefault();
                    if (sd.PropertyName != sortProperty || sd.Direction != (currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending))
                    {
                        this.LogsView.SortDescriptions.Clear();
                        this.LogsView.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                    }
                }

                if (this.LogsView.GroupDescriptions.Any())
                {
                    var gpn = ((PropertyGroupDescription)this.LogsView.GroupDescriptions[0]).PropertyName;
                    if (this.LogsView.GroupDescriptions.Any() && this.LogsView.SortDescriptions.All(sd => sd.PropertyName != gpn))
                    {
                        this.LogsView.SortDescriptions.Insert(0, new SortDescription(gpn, ListSortDirection.Ascending));
                    }
                }
            }
        }
        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.EventsLoading)
            {
                return;
            }

            this.Progress.Progress = e.Progress;
            this.Progress.Message = e.Message;
            this.Progress.IsLoading = e.IsBusy;
        }

        public ProgressProperty Progress { get; }

        private void OnSetEventViewerFilterCommand(UiExecutedPayload<SetEventViewerFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
            this.LogsView.Refresh();
        }

        private void OnSetEventViewerSortingCommand(UiExecutedPayload<SetEventViewerSortingCommand> obj)
        {
            this.SetSortingAndGrouping();
        }

        private void OnGetLogs(UiExecutedPayload<GetLogsCommand, IList<Log>> obj)
        {
            this.Logs.Clear();
            
            foreach (var item in obj.Result)
            {
                this.Logs.Add(new LogViewModel(item));
            }
        }
        
        public string SearchKey
        {
            get => this.application.ApplicationState.EventViewer.SearchKey;
            set
            {
                var currentFilter = this.application.ApplicationState.EventViewer.Filter;
                this.application.CommandExecutor.Invoke(this, new SetEventViewerFilterCommand(currentFilter, value));
            }
        }
        
        public ObservableCollection<LogViewModel> Logs { get; }

        public ICollectionView LogsView { get; }
        
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int MaxLogs { get; set; }
        
        public void Sort(string columnName, bool descending)
        {
            this.LogsView.SortDescriptions.Clear();
            this.LogsView.SortDescriptions.Add(new SortDescription(columnName, descending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }

        private bool Filter(object obj)
        {
            var filtered = (LogViewModel)obj;

            var filterLevel = this.application.ApplicationState.EventViewer.Filter & EventFilter.AllLevels;
            if (filterLevel != 0)
            {
                switch (filtered.Level)
                {
                    case "Error":
                        if (!filterLevel.HasFlag(EventFilter.Error))
                        {
                            return false;
                        }

                        break;
                    case "Warning":
                        if (!filterLevel.HasFlag(EventFilter.Warning))
                        {
                            return false;
                        }

                        break;
                    case "Information":
                        if (!filterLevel.HasFlag(EventFilter.Info))
                        {
                            return false;
                        }

                        break;
                    case "Verbose":
                        if (!filterLevel.HasFlag(EventFilter.Verbose))
                        {
                            return false;
                        }

                        break;
                }
            }

            var searchKey = this.application.ApplicationState.EventViewer.SearchKey;
            if (!string.IsNullOrEmpty(searchKey))
            {
                var searchMatch = false;
                if (filtered.OpcodeDisplayName != null)
                {
                    searchMatch = filtered.OpcodeDisplayName.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) != -1;
                }

                if (!searchMatch && filtered.Message != null)
                {
                    searchMatch = filtered.Message.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) != -1;
                }

                if (!searchMatch)
                {
                    return false;
                }
            }
            
            return true;
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                this.IsActiveChanged?.Invoke(this, EventArgs.Empty);

                if (value && this.firstRun)
                {
#pragma warning disable 4014
                    this.SetInitialData();
#pragma warning restore 4014
                }
            }
        }

        public event EventHandler IsActiveChanged;

        private async Task SetInitialData()
        {
            using var cts = new CancellationTokenSource();
            using var task = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.EventsLoading)
                .WithErrorHandling(this.interactionService, true)
                .Invoke<GetLogsCommand, IList<Log>>(this, new GetLogsCommand(), cts.Token);

            this.Progress.MonitorProgress(task, cts);
            await task.ConfigureAwait(false);
        }
    }
}

