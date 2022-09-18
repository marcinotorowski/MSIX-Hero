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

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.EventViewer.Search.ViewModels
{
    public enum ClearFilter
    {
        Level
    }

    public class EventViewerFilterSortViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;
        private readonly IInteractionService _interactionService;
        private readonly IBusyManager _busyManager;

        public EventViewerFilterSortViewModel(
            IMsixHeroApplication application, 
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this._application = application;
            this._interactionService = interactionService;
            this._busyManager = busyManager;

            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerFilterCommand>>().Subscribe(this.OnSetEventViewerFilter);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerSortingCommand>>().Subscribe(this.OnSetEventViewerSorting);
            
            this._application.EventAggregator.GetEvent<UiCancelledEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommand);
            this._application.EventAggregator.GetEvent<UiFailedEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommand);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetEventsCommand>>().Subscribe(this.OnGetLogsCommand);
            
            this.Clear = new DelegateCommand<object>(this.OnClearFilter);
        }

        public ICommand Clear { get; }

        public bool IsDescending
        {
            get => this._application.ApplicationState.EventViewer.SortDescending;
            set => this._application.CommandExecutor.Invoke(this, new SetEventViewerSortingCommand(this.Sort, value));
        }

        public EventSort Sort
        {
            get => this._application.ApplicationState.EventViewer.SortMode;
            set => this._application.CommandExecutor.Invoke(this, new SetEventViewerSortingCommand(value, this.IsDescending));
        }

        public bool FilterError
        {
            get => this._application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Error);
            set => this.SetEventViewerFilter(EventFilter.Error, value);
        }

        public LogCriteriaTimeSpan TimeSpan
        {
            get => this._application.ApplicationState.EventViewer.Criteria.TimeSpan ?? LogCriteriaTimeSpan.LastDay;
            set
            {
                if (this._application.ApplicationState.EventViewer.Criteria.TimeSpan != value)
                {
                    this.Load(value);
                }
            }
        }

        private async void Load(LogCriteriaTimeSpan value)
        {
            var executor = this._application.CommandExecutor
                .WithErrorHandling(this._interactionService, true)
                .WithBusyManager(this._busyManager, OperationType.EventsLoading);

            await executor.Invoke<GetEventsCommand, IList<AppxEvent>>(this, new GetEventsCommand(value), CancellationToken.None).ConfigureAwait(false);
        }

        public bool FilterWarning
        {
            get => this._application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Warning);
            set => this.SetEventViewerFilter(EventFilter.Warning, value);
        }

        public bool FilterVerbose
        {
            get => this._application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Verbose);
            set => this.SetEventViewerFilter(EventFilter.Verbose, value);
        }

        public bool FilterInfo
        {
            get => this._application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Info);
            set => this.SetEventViewerFilter(EventFilter.Info, value);
        }

        public string FilterLevelCaption
        {
            get
            {
                var val = this._application.ApplicationState.EventViewer.Filter & EventFilter.AllLevels;
                if (val == 0 || val == EventFilter.AllLevels)
                {
                    return Resources.Localization.FilterAll;
                }

                var selected = 0;
                if (this.FilterError)
                {
                    selected++;
                }

                if (this.FilterWarning)
                {
                    selected++;
                }

                if (this.FilterInfo)
                {
                    selected++;
                }

                if (this.FilterVerbose)
                {
                    selected++;
                }

                return $"({selected}/4)";
            }
        }
        
        private void OnSetEventViewerSorting(UiExecutedPayload<SetEventViewerSortingCommand> obj)
        {
            this.OnPropertyChanged(nameof(IsDescending));
            this.OnPropertyChanged(nameof(Sort));
        }

        private void OnSetEventViewerFilter(UiExecutedPayload<SetEventViewerFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(FilterError));
            this.OnPropertyChanged(nameof(FilterInfo));
            this.OnPropertyChanged(nameof(FilterWarning));
            this.OnPropertyChanged(nameof(FilterVerbose));
            this.OnPropertyChanged(nameof(FilterLevelCaption));
        }

        private void OnGetLogsCommand(UiPayload<GetEventsCommand> obj)
        {
            this.OnPropertyChanged(nameof(this.TimeSpan));
        }

        private void SetEventViewerFilter(EventFilter eventFilter, bool isSet)
        {
            var currentFilter = this._application.ApplicationState.EventViewer.Filter;
            if (isSet)
            {
                currentFilter |= eventFilter;
            }
            else
            {
                currentFilter &= ~eventFilter;
            }

            this.SetEventViewerFilter(currentFilter);
        }

        private void SetEventViewerFilter(EventFilter filter)
        {
            var state = this._application.ApplicationState.EventViewer;
            this._application.CommandExecutor.WithErrorHandling(this._interactionService, false).Invoke(this, new SetEventViewerFilterCommand(filter, state.SearchKey));
        }
        
        private void OnClearFilter(object objectFilterToClear)
        {
            if (!(objectFilterToClear is ClearFilter filterToClear))
            {
                return;
            }

            switch (filterToClear)
            {
                case ClearFilter.Level:
                    this.SetEventViewerFilter(EventFilter.All, true);
                    break;
            }
        }
    }
}
