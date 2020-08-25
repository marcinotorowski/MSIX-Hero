using System.Collections.Generic;
using System.Linq;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.ThirdParty
{
    public class AdvancedInstallerExpressAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            // Detect MSIX version

            var pkgMan = new PackageManager();
            var pkg = pkgMan.FindPackagesForUser(string.Empty, "Caphyon.AdvancedInstallerExpress_3f31d3yrednja").FirstOrDefault();

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