// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.State
{
    public class EventViewerState
    {
        public EventViewerState()
        {
            this.Filter = EventFilter.Warning | EventFilter.Error; // default show only warning and errors
            this.SortMode = EventSort.Date;
            this.SortDescending = true;
            this.Criteria = new EventCriteria
            {
                TimeSpan = LogCriteriaTimeSpan.LastDay
            };
        }

        public string SearchKey { get; set; }

        public AppxEvent SelectedAppxEvent { get; set; }

        public EventSort SortMode { get; set; }
        
        public bool SortDescending { get; set; }
        
        public EventFilter Filter { get; set; }

        public EventCriteria Criteria { get; set; }
    }
}