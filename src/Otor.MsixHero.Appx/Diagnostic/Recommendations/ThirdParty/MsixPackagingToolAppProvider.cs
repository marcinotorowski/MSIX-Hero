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
            var pkg  = AppxPackageManager.PackageManager.Value.FindPackagesForUser(string.Empty, "Microsoft.MsixPackagingTool_8wekyb3d8bbwe").FirstOrDefault();

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