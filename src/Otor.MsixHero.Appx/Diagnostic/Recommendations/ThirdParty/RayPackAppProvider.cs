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
