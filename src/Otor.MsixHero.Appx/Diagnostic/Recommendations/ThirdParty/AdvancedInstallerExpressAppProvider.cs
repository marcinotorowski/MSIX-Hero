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
using Otor.MsixHero.Appx.Packaging.Installation;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty
{
    public class AdvancedInstallerExpressAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            // Detect MSIX version
            var pkg = AppxPackageManager.PackageManager.Value.FindPackagesForUser(string.Empty, "Caphyon.AdvancedInstallerExpress_3f31d3yrednja").FirstOrDefault();

            if (pkg == null)
            {
                yield return new ThirdPartyStoreApp("ADVINST", "Advanced Installer Express", "Caphyon", "https://msixhero.net/redirect/ms-store/advanced-installer-express", "Caphyon.AdvancedInstallerExpress_3f31d3yrednja");
            }
            else
            {
                var ver = $"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}";
                yield return new ThirdPartyDetectedStoreApp("ADVINST", "Advanced Installer Express", "Caphyon", ver, "https://msixhero.net/redirect/ms-store/advanced-installer-express", "Caphyon.AdvancedInstallerExpress_3f31d3yrednja");
            }
        }
    }
}