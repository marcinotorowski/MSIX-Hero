using System;
using System.IO;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;

namespace Otor.MsixHero.Appx.Packaging
{
    public enum PackageTypeDisplay
    {
        Long,
        Normal,
        Short
    }

    public static class PackageTypeConverter
    {
        public static string GetPackageTypeStringFrom(MsixPackageType packageType, PackageTypeDisplay displayType = PackageTypeDisplay.Normal)
        {
            var isUwp = (packageType & MsixPackageType.Uwp) == MsixPackageType.Uwp;
            if (isUwp)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return "Universal Windows Platform (UWP) app";
                    default:
                        return "UWP";
                }
            }

            var isBridge = (packageType & MsixPackageType.BridgeDirect) == MsixPackageType.BridgeDirect;
            if (isBridge)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return "Classic Win32 app";
                    default:
                        return "Win32";
                }
            }

            var isPsf = (packageType & MsixPackageType.BridgePsf) == MsixPackageType.BridgePsf;
            if (isPsf)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return "Classic Win32 app enhanced by Package Support Framework (PSF)";
                    case PackageTypeDisplay.Short:
                        return "PSF";
                    default:
                        return "Win32 + PSF";
                }
            }

            var isWeb = (packageType & MsixPackageType.Web) == MsixPackageType.Web;
            if (isWeb)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return "Web application";
                    default:
                        return "Web";
                }
            }

            var isFramework = (packageType & MsixPackageType.Framework) == MsixPackageType.Framework;
            if (isFramework)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Short:
                        return "FRAMEWORK";
                    default:
                        return "Framework";
                }
            }

            switch (displayType)
            {
                case PackageTypeDisplay.Short:
                    return "App";
                default:
                    return "Unknown";
            }
        }

        public static string GetPackageTypeStringFrom(string entryPoint, string executable, string startPage, bool isFramework, PackageTypeDisplay displayType = PackageTypeDisplay.Normal)
        {
            return GetPackageTypeStringFrom(GetPackageTypeFrom(entryPoint, executable, startPage, isFramework), displayType);
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

                        if (!string.IsNullOrEmpty(executable) && string.Equals(".exe", Path.GetExtension(executable).ToLowerInvariant()))
                        {
                            executable = "\\" + executable; // to make sure we have a backslash for checking

                            if (
                                executable.IndexOf("\\psflauncher", StringComparison.OrdinalIgnoreCase) != -1 ||
                                executable.IndexOf("\\psfrundll", StringComparison.OrdinalIgnoreCase) != -1 ||
                                executable.IndexOf("\\ai_stubs", StringComparison.OrdinalIgnoreCase) != -1 ||
                                executable.IndexOf("\\psfmonitor", StringComparison.OrdinalIgnoreCase) != -1)
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
