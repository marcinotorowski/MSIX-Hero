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

using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Tooling
{
    public class DiscoveredAppViewModel
    {
        public DiscoveredAppViewModel(IThirdPartyApp app, DiscoveredAppViewModelStatus status = DiscoveredAppViewModelStatus.Unknown)
        {
            this.Status = status;
            this.IconId = app.AppId;
            this.Name = app.Name;
            this.Publisher = app.Publisher;
            this.Website = app.Website;
        }

        public DiscoveredAppViewModel(IThirdPartyDetectedApp app, DiscoveredAppViewModelStatus status = DiscoveredAppViewModelStatus.Unknown)
        {
            this.Status = status;
            this.IconId = app.AppId;
            this.Name = app.Name;
            this.Publisher = app.Publisher;
            this.Website = app.Website;
            this.Version = app.Version;
        }

        public DiscoveredAppViewModelStatus Status { get; }

        public string IconId { get; }

        public string Name { get; }

        public string Publisher { get; }
        
        public string Version { get; }

        public string Website { get; }
    }
}
