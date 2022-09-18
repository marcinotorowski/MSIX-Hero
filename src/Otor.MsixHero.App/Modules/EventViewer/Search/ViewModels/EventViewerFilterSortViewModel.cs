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

using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
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
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;

        public EventViewerFilterSortViewModel(IMsixHeroApplication application, IInteractionService interactionService)
        {
            this.application = application;
            this.interactionService = interactionService;

            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerFilterCommand>>().Subscribe(this.OnSetEventViewerFilter);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetEventViewerSortingCommand>>().Subscribe(this.OnSetEventViewerSorting);

            this.Clear = new DelegateCommand<object>(this.OnClearFilter);
        }

        public ICommand Clear { get; }

        public bool IsDescending
        {
            get => this.application.ApplicationState.EventViewer.SortDescending;
            set => this.application.CommandExecutor.Invoke(this, new SetEventViewerSortingCommand(this.Sort, value));
        }

        public EventSort Sort
        {
            get => this.application.ApplicationState.EventViewer.SortMode;
            set => this.application.CommandExecutor.Invoke(this, new SetEventViewerSortingCommand(value, this.IsDescending));
        }

        public bool FilterError
        {
            get => this.application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Error);
            set => this.SetEventViewerFilter(EventFilter.Error, value);
        }

        public bool FilterWarning
        {
            get => this.application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Warning);
            set => this.SetEventViewerFilter(EventFilter.Warning, value);
        }

        public bool FilterVerbose
        {
            get => this.application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Verbose);
            set => this.SetEventViewerFilter(EventFilter.Verbose, value);
        }

        public bool FilterInfo
        {
            get => this.application.ApplicationState.EventViewer.Filter.HasFlag(EventFilter.Info);
            set => this.SetEventViewerFilter(EventFilter.Info, value);
        }

        public string FilterLevelCaption
        {
            get
            {
                var val = this.application.ApplicationState.EventViewer.Filter & EventFilter.AllLevels;
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

        private void SetEventViewerFilter(EventFilter eventFilter, bool isSet)
        {
            var currentFilter = this.application.ApplicationState.EventViewer.Filter;
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
            var state = this.application.ApplicationState.EventViewer;
            this.application.CommandExecutor.WithErrorHandling(this.interactionService, false).Invoke(this, new SetEventViewerFilterCommand(filter, state.SearchKey));
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
