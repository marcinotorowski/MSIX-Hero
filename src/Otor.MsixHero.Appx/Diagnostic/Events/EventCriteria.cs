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

using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.Appx.Diagnostic.Events;

public class EventCriteria
{
    public EventCriteria()
    {
    }

    public EventCriteria(int maxCount)
    {
        this.MaxCount = maxCount;
    }

    public EventCriteria(LogCriteriaTimeSpan timeSpan)
    {
        this.TimeSpan = timeSpan;
    }
    
    public int? MaxCount { get; set; }

    public LogCriteriaTimeSpan? TimeSpan { get; set; }
    
}