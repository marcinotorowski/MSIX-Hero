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

using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty
{
    public class ThirdPartyAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            var detectors = new List<IThirdPartyAppProvider>
            {
                new MsixHeroAppProvider(),
                new AdvancedInstallerExpressAppProvider(),
                new MsixPackagingToolAppProvider(),
                new RayPackAppProvider()
            };

            return detectors.SelectMany(d => d.ProvideApps()).Where(a => a.AppId != "MSIXHERO").OrderBy(a => a.AppId == "MSIXPKGTOOL" ? "#" : a.Name);
        }
    }
}
