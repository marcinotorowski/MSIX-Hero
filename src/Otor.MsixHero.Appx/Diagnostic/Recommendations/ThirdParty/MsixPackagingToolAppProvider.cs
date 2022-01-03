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
using System.Linq;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Packaging.Installation;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty
{
    public class MsixPackagingToolAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            // Detect MSIX version
            var pkg = PackageManagerWrapper.Instance.FindPackagesForUser(string.Empty, "Microsoft.MsixPackagingTool_8wekyb3d8bbwe").FirstOrDefault();

            if (pkg == null)
            {
                yield return new ThirdPartyStoreApp("MSIXPKGTOOL", "MSIX Packaging Tool", "Microsoft Corporation",  "https://msixhero.net/redirect/ms-store/msix-packaging-tool", "Microsoft.MsixPackagingTool_8wekyb3d8bbwe");
            }
            else
            {
                var ver = $"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}";
                yield return new ThirdPartyDetectedStoreApp("MSIXPKGTOOL", "MSIX Packaging Tool", "Microsoft Corporation", ver, "https://msixhero.net/redirect/ms-store/msix-packaging-tool", "Microsoft.MsixPackagingTool_8wekyb3d8bbwe");
            }
        }
    }
}