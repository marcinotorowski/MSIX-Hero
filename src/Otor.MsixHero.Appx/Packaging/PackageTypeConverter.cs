// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
                        return Resources.Localization.Packages_Types_UWP_Long;
                    default:
                        return Resources.Localization.Packages_Types_UWP;
                }
            }

            var isBridge = (packageType & MsixPackageType.BridgeDirect) == MsixPackageType.BridgeDirect;
            if (isBridge)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return Resources.Localization.Packages_Types_W32_Long;
                    default:
                        return Resources.Localization.Packages_Types_W32;
                }
            }

            var isPsf = (packageType & MsixPackageType.BridgePsf) == MsixPackageType.BridgePsf;
            if (isPsf)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return Resources.Localization.Packages_Types_Psf_Long;
                    case PackageTypeDisplay.Short:
                        return Resources.Localization.Packages_Types_Psf_Short;
                    default:
                        return Resources.Localization.Packages_Types_Psf;
                }
            }

            var isWeb = (packageType & MsixPackageType.Web) == MsixPackageType.Web;
            if (isWeb)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Long:
                        return Resources.Localization.Packages_Types_Pwa_Long;
                    default:
                        return Resources.Localization.Packages_Types_Pwa;
                }
            }

            var isFramework = (packageType & MsixPackageType.Framework) == MsixPackageType.Framework;
            if (isFramework)
            {
                switch (displayType)
                {
                    case PackageTypeDisplay.Short:
                        return Resources.Localization.Packages_Types_Framework;
                    default:
                        return Resources.Localization.Packages_Types_Framework;
                }
            }

            switch (displayType)
            {
                case PackageTypeDisplay.Short:
                    return Resources.Localization.Packages_Types_App;
                default:
                    return Resources.Localization.Packages_Types_Unknown;
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
