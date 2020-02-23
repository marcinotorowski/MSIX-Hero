using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.BusinessLayer.Helpers
{
    public static class PackageTypeConverter
    {
        public static string GetPackageTypeStringFrom(MsixPackageType packageType)
        {
            var isUwp = (packageType & MsixPackageType.Uwp) == MsixPackageType.Uwp;
            if (isUwp)
            {
                return "UWP";
            }

            var isBridge = (packageType & MsixPackageType.BridgeDirect) == MsixPackageType.BridgeDirect;
            if (isBridge)
            {
                return "Win32";
            }

            var isPsf = (packageType & MsixPackageType.BridgePsf) == MsixPackageType.BridgePsf;
            if (isPsf)
            {
                return "Win32 + PSF";
            }

            var isWeb = (packageType & MsixPackageType.Web) == MsixPackageType.Web;
            if (isWeb)
            {
                return "Web";
            }

            var isFramework = (packageType & MsixPackageType.Framework) == MsixPackageType.Framework;
            if (isFramework)
            {
                return "Framework";
            }

            return "Unknown";
        }

        public static string GetPackageTypeStringFrom(string entryPoint, string executable, string startPage, bool isFramework)
        {
            return GetPackageTypeStringFrom(GetPackageTypeFrom(entryPoint, executable, startPage, isFramework));
        }

        public static MsixPackageType GetPackageTypeFrom(string entryPoint, string executable, string startPage, bool isFramework)
        {
            if (isFramework)
            {
                return MsixPackageType.Framework;
            }

            if (!string.IsNullOrEmpty(entryPoint))
            {
                switch (entryPoint)
                {
                    case "Windows.FullTrustApplication":

                        if (!string.IsNullOrEmpty(executable))
                        {
                            if (string.Equals("psflauncher32.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psflauncher64.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psflauncher32.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psflauncher.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psfrundll64.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psfrundll32.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psfrundll.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("ai_stubs\\aistub.exe", executable, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals("psfmonitor.exe", executable, StringComparison.OrdinalIgnoreCase))
                            {
                                return MsixPackageType.BridgePsf;
                            }

                            return MsixPackageType.BridgeDirect;
                        }

                        return 0;
                }

                if (string.IsNullOrEmpty(startPage))
                {
                    return MsixPackageType.Uwp;
                }

                return 0;
            }

            return string.IsNullOrEmpty(startPage) ? 0 : MsixPackageType.Web;
        }
    }
}
