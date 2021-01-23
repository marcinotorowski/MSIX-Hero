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

using System;
using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Packaging.Installation;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty
{
    public class RayPackAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            // Detect MSIX version
            var pkg  = AppxPackageManager.PackageManager.Value.FindPackagesForUser(string.Empty, "11560RaynetGmbH.RayPack_whm4wqzjy81pg").FirstOrDefault();
            if (pkg == null)
            {
                yield return new ThirdPartyStoreApp("RAYPACK", "RayPack", "Raynet GmbH", "https://msixhero.net/redirect/ms-store/raypack", "11560RaynetGmbH.RayPack_whm4wqzjy81pg");
            }
            else
            {
                yield return new RayPackApp($"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}");
            }
        }

        private class RayPackApp : ThirdPartyDetectedStoreApp, IMsixCreator
        {
            public RayPackApp(string version) : base("RAYPACK", "RayPack", "Raynet GmbH", version, "https://msixhero.net/redirect/ms-store/raypack", "11560RaynetGmbH.RayPack_whm4wqzjy81pg")
            {
            }
            
            public string ProjectExtension { get; } = "rppx";

            public void CreateProject(string path, bool open = true)
            {
                throw new NotImplementedException();
            }
        }
    }
}
