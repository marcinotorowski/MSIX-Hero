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

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                new RayPackAppProvider(),
                new PsfToolingAppProvider()
            };

            unchecked
            {
                // this is to ensure that we have a pseudo-random order
                // which is constant for a particular session.
                // Also, add hashcode of the machine so that we get unique
                // results even if PIDs are the same.
                var seed = Process.GetCurrentProcess().Id;
                seed = (seed * 39) ^ Environment.MachineName.GetHashCode();

                var rnd = new Random(seed);
                
                return detectors
                    .SelectMany(d => d.ProvideApps())
                    .Where(a => a.AppId != "MSIXHERO")
                    .OrderBy(a =>
                    {
                        switch (a)
                        {
                            case ThirdPartyDetectedStoreApp:
                                return 0;
                            case IThirdPartyDetectedApp:
                                return 1;
                            case IStoreApp:
                                return 2;
                            default:
                                return 3;
                        }
                    })
                    .ThenBy(d => rnd.Next());
            }
        }
    }
}
