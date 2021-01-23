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

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities
{
    public class ThirdPartyApp : IThirdPartyApp
    {
        public ThirdPartyApp(string appId, string name, string publisher, string website)
        {
            this.AppId = appId;
            this.Name = name;
            this.Publisher = publisher;
            this.Website = website;
        }

        public string AppId { get; }

        public string Name { get; }

        public string Publisher { get; }

        public string Website { get; }
    }
}