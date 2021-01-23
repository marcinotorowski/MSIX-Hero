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
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.EventViewer.Details.ViewModels
{
    public class EventViewerDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private LogViewModel selectedLog;

        public EventViewerDetailsViewModel()
        {
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var newSelection = GetLogFromContext(navigationContext);

            if (newSelection == null)
            {
                this.SelectedLog = null;
            }
            else if (this.SelectedLog?.Model != newSelection)
            {
                this.SelectedLog = new LogViewModel(newSelection);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public LogViewModel SelectedLog
        {
            get => this.selectedLog;
            private set => this.SetField(ref this.selectedLog, value);
        }

        private static Log GetLogFromContext(NavigationContext context)
        {
            var key = context.Parameters.Keys.FirstOrDefault(k => context.Parameters[k] is Log);
            if (key == null)
            {
                return null;
            }

            return (Log) context.Parameters[key];
        }
    }
}

