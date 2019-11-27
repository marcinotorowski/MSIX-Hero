using System;
using otor.msixhero.lib.BusinessLayer.Models.Packages;

namespace otor.msixhero.lib.BusinessLayer.Helpers
{
    public static class PackageTypeConverter
    {
        public static string GetPackageTypeStringFrom(PackageType packageType)
        {
            var isUwp = (packageType & PackageType.Uwp) == PackageType.Uwp;
            var isPsf = (packageType & PackageType.BridgePsf) == PackageType.BridgePsf;
            var isBridge = (packageType & PackageType.BridgeDirect) == PackageType.BridgeDirect;
            var isWeb = (packageType & PackageType.Web) == PackageType.Web;

            if (isWeb)
            {
                return "Web";
            }

            if (isUwp)
            {
                return isPsf ? "UWP + PSF" : "UWP";
            }

            if (isPsf)
            {
                return "Desktop Bridge + PSF";
            }

            if (isBridge)
            {
                return "Desktop Bridge";
            }

            return "Unknown";
        }

        public static string GetPackageTypeStringFrom(string entryPoint, string executable, string startPage)
        {
            return GetPackageTypeStringFrom(GetPackageTypeFrom(entryPoint, executable, startPage));
        }

        public static PackageType GetPackageTypeFrom(string entryPoint, string executable, string startPage)
        {
            if (string.IsNullOrEmpty(entryPoint))
            {
                return 0;
            }

            switch (entryPoint)
            {
                case "Windows.FullTrustApplication":

                    if (!string.IsNullOrEmpty(executable))
                    {
                        if (string.Equals("psflauncher32.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("psflauncher64.exe", executable, StringComparison.OrdinalIgnoreCase) || 
                            string.Equals("psflauncher32.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("psfrundll64.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals("psfrundll32.exe", executable, StringComparison.OrdinalIgnoreCase) || 
                            string.Equals("psfrundll.exe", executable, StringComparison.OrdinalIgnoreCase) || 
                            string.Equals("psfmonitor.exe", executable, StringComparison.OrdinalIgnoreCase))
                        {
                            return PackageType.BridgePsf;
                        }

                        return PackageType.BridgeDirect;
                    }

                    return 0;
            }

            if (string.IsNullOrEmpty(startPage))
            {
                return PackageType.Uwp;
            }

            return PackageType.Web;
        }
    }
}
