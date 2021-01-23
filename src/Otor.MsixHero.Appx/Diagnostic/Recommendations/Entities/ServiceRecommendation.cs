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

using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public class ServiceRecommendation : IServiceRecommendation
    {
        public ServiceRecommendation(string serviceName, string displayName, string actionPrompt, bool expectedToRun, bool actual, ServiceRecommendationType type = ServiceRecommendationType.Service)
        {
            this.ActionPrompt = actionPrompt;
            this.Actual = actual;
            this.Type = type;
            this.ExpectedToRun = expectedToRun;
            this.DisplayName = displayName;
            this.ServiceName = serviceName;
        }

        public string ActionPrompt { get; }

        public bool Actual { get; }

        public ServiceRecommendationType Type { get; }

        public bool ExpectedToRun { get; }

        public string DisplayName { get; }

        public string ServiceName { get; }
    }
}