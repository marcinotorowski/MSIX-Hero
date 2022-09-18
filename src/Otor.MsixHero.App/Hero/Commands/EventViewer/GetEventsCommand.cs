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

using System.Collections.Generic;
using MediatR;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.EventViewer
{
    public class GetEventsCommand : IRequest<IList<AppxEvent>>
    {
        public GetEventsCommand()
        {
            this.Criteria = new EventCriteria();
        }

        public GetEventsCommand(EventCriteria eventCriteria)
        {
            this.Criteria = eventCriteria;
        }

        public GetEventsCommand(LogCriteriaTimeSpan timeSpan)
        {
            this.Criteria = new EventCriteria
            {
                TimeSpan = timeSpan
            };
        }

        public EventCriteria Criteria { get; }
    }
}
