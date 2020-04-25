using System;
using System.Collections.Generic;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public class RayPackDetector : IThirdPartyDetector
    {
        public IEnumerable<ThirdPartyDetectedApp> DetectApps()
        {
            // Detect MSIX version

            var pkgMan = new PackageManager();
            var raypack  = pkgMan.FindPackagesForUser(string.Empty, "11560RaynetGmbH.RayPack_whm4wqzjy81pg");

            foreach (var pkg in raypack)
            {
                yield return new RayPackApp($"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}");
            }
        }

        private class RayPackApp : ThirdPartyDetectedApp, IMsixCreator
        {
            public RayPackApp(string version) : base("RAYPACK", "RayPack", "Raynet GmbH", version, "https://www.microsoft.com/en-us/p/raypack/9n73s87k0txn")
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
