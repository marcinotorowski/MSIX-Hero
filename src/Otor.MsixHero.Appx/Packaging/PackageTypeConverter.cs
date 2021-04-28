// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Linq;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.Appx.Packaging
{
    public static class PackageTypeConverter
    {
        public enum PackageTypeDisplay
        {
            Long,
            Normal,
            Short
        }

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
                        return "Progressive Web Application";
                    default:
                        return "PWA";
                }
            }

            var isFramework = (packageType & MsixPackageType.Framework) == MsixPackageType.Framework;
            if (isFramework)
            {
                return "Framework";
            }

            var isHosted = (packageType & MsixPackageType.HostedApp) == MsixPackageType.HostedApp;
            if (isHosted)
            {
                return "Hosted app";
            }

            switch (displayType)
            {
                case PackageTypeDisplay.Short:
                    return "App";
                default:
                    return "Unknown";
            }
        }
        
        public static MsixPackageType GetPackageTypeFrom(string entryPoint, string executable, string startPage, string hostId, bool isFramework)
        {
            if (isFramework)
            {
                return MsixPackageType.Framework;
            }

            if (!string.IsNullOrEmpty(hostId))
            {
                return MsixPackageType.HostedApp;
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

        public static MsixPackageType GetPackageTypeFrom(AppxPackage package, AppxApplication application)
        {
            return GetPackageTypeFrom(application.EntryPoint, application.Executable, application.StartPage, application.HostId, package.IsFramework);
        }
        
        public static string GetTargetFrom(AppxPackage package, AppxApplication application)
        {
            switch (GetPackageTypeFrom(package, application))
            {
                case MsixPackageType.BridgeDirect:
                case MsixPackageType.BridgePsf:
                    return application.Executable;
                case MsixPackageType.Web:
                    return application.StartPage;
                case MsixPackageType.HostedApp:
                    string hostedDependency;
                    if (package.HostPackageDependencies?.Count == 1)
                    {
                        hostedDependency = package.HostPackageDependencies.First().Name + " [" + application.HostId + "]";
                    }
                    else
                    {
                        hostedDependency = application.HostId;
                    }

                    if (string.IsNullOrEmpty(application.Parameters))
                    {
                        return hostedDependency;
                    }

                    return hostedDependency + Environment.NewLine + application.Parameters;
                default:
                    if (string.IsNullOrEmpty(application.EntryPoint))
                    {
                        return application.Executable;
                    }

                    return application.Executable;
            }
        }
    }
}
