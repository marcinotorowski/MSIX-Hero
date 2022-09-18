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
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels
{
    public class EventViewerDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private EventViewModel _selectedEvent;

        public EventViewerDetailsViewModel()
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var newSelection = GetLogFromContext(navigationContext);

            if (newSelection == null)
            {
                this.SelectedEvent = null;
            }
            else if (this.SelectedEvent?.Model != newSelection)
            {
                this.SelectedEvent = new EventViewModel(newSelection);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public EventViewModel SelectedEvent
        {
            get => this._selectedEvent;
            private set => this.SetField(ref this._selectedEvent, value);
        }

        private static AppxEvent GetLogFromContext(NavigationContext context)
        {
            var key = context.Parameters.Keys.FirstOrDefault(k => context.Parameters[k] is AppxEvent);
            if (key == null)
            {
                return null;
            }

            return (AppxEvent) context.Parameters[key];
        }
    }
}

