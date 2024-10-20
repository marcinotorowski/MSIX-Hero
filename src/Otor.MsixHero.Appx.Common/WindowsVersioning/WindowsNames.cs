// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Appx.Common.WindowsVersioning
{
    public static class WindowsNames
    {
        private static readonly List<WindowsVersionHandler> VersionHandlers;

        static WindowsNames()
        {
            VersionHandlers = [
                // Windows 7 and Server 2008 R2
                new (VersionRange.Exact("6.1.7601"), version => new AppxTargetOperatingSystem("Windows 7 SP1 / Server 2008 R2", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win7) ),

                // Windows 8 and Server 2012
                new (VersionRange.Exact("6.2.9200"), version => new AppxTargetOperatingSystem("Windows 8 / Server 2012", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win8) ),

                // Windows 8.1 and Server 2012 R2
                new (VersionRange.Exact("6.3.9600"), version => new AppxTargetOperatingSystem("Windows 8.1 / Server 2012 R2", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win81) ),

                // Windows 10
                new (VersionRange.Exact("10.0.10240"), version => new AppxTargetOperatingSystem("Windows 10 1507", "RTM", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.10586"), version => new AppxTargetOperatingSystem("Windows 10 1511", "November Update", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.14393"), version => new AppxTargetOperatingSystem("Windows 10 1607", "Anniversary Update", AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.15063"), version => new AppxTargetOperatingSystem("Windows 10 1703", "Creators Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.16299"), version => new AppxTargetOperatingSystem("Windows 10 1709", "Fall Creators Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.17134"), version => new AppxTargetOperatingSystem("Windows 10 1803", "April 2018 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.17763"), version => new AppxTargetOperatingSystem("Windows 10 1809", "October 2018 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.18362"), version => new AppxTargetOperatingSystem("Windows 10 1903", "May 2019 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.18363"), version => new AppxTargetOperatingSystem("Windows 10 1909", "November 2019 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.19041"), version => new AppxTargetOperatingSystem("Windows 10 2004", "May 2020 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.19042"), version => new AppxTargetOperatingSystem("Windows 10 20H2", "October 2020 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                new (VersionRange.Exact("10.0.19043"), version => new AppxTargetOperatingSystem("Windows 10 21H1", "May 2021 Update", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win10) ),
                
                // Windows 11
                // https://learn.microsoft.com/en-us/windows/release-health/windows11-release-information
                new (VersionRange.Between("10.0.22000", true, "10.0.22000.3260", true), version => new AppxTargetOperatingSystem("Windows 11 21H2", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win11) ),
                new (VersionRange.Between("10.0.22000.3260", false, "10.0.22621.4317", true), version => new AppxTargetOperatingSystem("Windows 11 22H2", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win11) ),
                new (VersionRange.Between("10.0.22621.4317", false, "10.0.22631.4317", true), version => new AppxTargetOperatingSystem("Windows 11 23H2", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win11) ),
                new (VersionRange.Between("10.0.22631.4317", false, "10.0.26100.2033", true), version => new AppxTargetOperatingSystem("Windows 11 24H2", AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win11) ),

                // Generic Windows 7 builds
                new (VersionRange.Between("6.1.0", true, "6.2.0", false), version => new AppxTargetOperatingSystem("Windows 7 SP1 / Server 2008 R2 Build " + version.Build, AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win7) ),

                // Generic Windows 8 builds
                new (VersionRange.Between("6.2.0", true, "6.3.0", false), version => new AppxTargetOperatingSystem("Windows 8 / Server 2012 Build " + version.Build, AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win8) ),

                // Generic Windows 8.1 builds
                new (VersionRange.Between("6.3.0", true, "10.0.0", false), version => new AppxTargetOperatingSystem("Windows 8.1 / Server 2012 R2 Build " + version.Build, AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win81) ),

                // Generic Windows 10 builds
                new (VersionRange.Between("10.0.0", true, "10.0.22000", false), version => new AppxTargetOperatingSystem("Windows 10 Build " + version.Build, version.Build >= 15063 ? AppxTargetOperatingSystemType.MsixNativeSupported : AppxTargetOperatingSystemType.MsixCore, WindowsVersion.Win10) ),

                // Future Windows 11 builds
                new (VersionRange.HigherThan("10.0.26100.2033"), version => new AppxTargetOperatingSystem("Windows 11 Build " + version.Build, AppxTargetOperatingSystemType.MsixNativeSupported, WindowsVersion.Win11) )
            ];
        }

        public static AppxTargetOperatingSystem GetOperatingSystemFromNameAndVersion(string name, string version)
        {
            AppxTargetOperatingSystem result;

            switch (name)
            {
                case "Windows.Desktop":
                    result = GetWindowsDesktop(version);
                    break;
                case "Windows.Universal":
                    result = GetWindowsDesktop(version);
                    break;
                case "MSIXCore.Desktop":
                    result = GetMsixCoreDesktop(version);
                    break;
                default:
                    result= new AppxTargetOperatingSystem
                    {
                        IsNativeMsixPlatform = AppxTargetOperatingSystemType.Other, 
                        Name = name,
                        TechnicalVersion = version
                    };
                    break;
            }

            result.NativeFamilyName = name;
            return result;
        }
        
        public static AppxTargetOperatingSystem GetOperatingSystemFromNameAndVersion(string version)
        {
            return GetOperatingSystemFromNameAndVersion("Windows.Desktop", version);
        }

        private static AppxTargetOperatingSystem GetMsixCoreDesktop(string version)
        {
            var result = GetWindowsDesktop(version);
            result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
            return result;
        }

        public static AppxTargetOperatingSystem GetWindowsDesktop(string version)
        {
            if (!Version.TryParse(version, out var parsedVersion))
            {
                return new AppxTargetOperatingSystem
                {
                    TechnicalVersion = version,
                    Name = "Windows " + version,
                    IsNativeMsixPlatform = AppxTargetOperatingSystemType.Other,
                    WindowsVersion = WindowsVersion.Unspecified
                };
            }

            var findHandler = VersionHandlers.FirstOrDefault(vh => vh.VersionRange.Matches(parsedVersion));
            if (findHandler == null)
            {
                return new AppxTargetOperatingSystem
                {
                    TechnicalVersion = version,
                    Name = "Windows " + parsedVersion.ToString(3),
                    IsNativeMsixPlatform = AppxTargetOperatingSystemType.Other,
                    WindowsVersion = WindowsVersion.Unspecified
                };
            }

            var result = findHandler.NameGenerator(parsedVersion);
            result.TechnicalVersion = version;

            return result;
        }
    }
}
