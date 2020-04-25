using System.Collections.Generic;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public class AdvancedInstallerExpressDetector : IThirdPartyDetector
    {
        public IEnumerable<ThirdPartyDetectedApp> DetectApps()
        {
            // Detect MSIX version

            var pkgMan = new PackageManager();
            var raypack  = pkgMan.FindPackagesForUser(string.Empty, "Caphyon.AdvancedInstallerExpress_3f31d3yrednja");

            foreach (var pkg in raypack)
            {
                var ver = $"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}";
                yield return new ThirdPartyDetectedApp("ADVINST", "Advanced Installer Express", "Caphyon", ver, "https://www.microsoft.com/en-us/p/advanced-installer-express/9n4vqdj7ltb8");
            }
        }
    }
}