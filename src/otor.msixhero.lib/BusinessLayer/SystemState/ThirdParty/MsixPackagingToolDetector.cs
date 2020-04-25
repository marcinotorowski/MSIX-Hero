using System.Collections.Generic;
using Windows.Management.Deployment;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public class MsixPackagingToolDetector : IThirdPartyDetector
    {
        public IEnumerable<ThirdPartyDetectedApp> DetectApps()
        {
            // Detect MSIX version

            var pkgMan = new PackageManager();
            var raypack  = pkgMan.FindPackagesForUser(string.Empty, "Microsoft.MsixPackagingTool_8wekyb3d8bbwe");

            foreach (var pkg in raypack)
            {
                var ver = $"{pkg.Id.Version.Major}.{pkg.Id.Version.Minor}.{pkg.Id.Version.Build}.{pkg.Id.Version.Revision}";
                yield return new ThirdPartyDetectedApp("MSIXPKGTOOL", "MSIX Packaging Tool", "Microsoft", ver, "https://www.microsoft.com/en-us/p/msix-packaging-tool/9n5lw3jbcxkf");
            }
        }
    }
}