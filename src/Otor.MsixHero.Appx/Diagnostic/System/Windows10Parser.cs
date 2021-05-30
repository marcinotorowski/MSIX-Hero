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
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.System
{
    public class Windows10Parser
    {
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

        private static AppxTargetOperatingSystem GetWindowsDesktop(string version)
        {
            var result = new AppxTargetOperatingSystem();
            result.TechnicalVersion = version;

            if (version != null && Version.TryParse(version, out var parsedVersion))
            {
                version = parsedVersion.ToString(3);
                switch (version)
                {
                    case "6.1.7601":
                        result.Name = "Windows 7 SP1 / Server 2008 R2";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "6.2.9200":
                        result.Name = "Windows Server 2012";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "6.3.9600":
                        result.Name = "Windows 8.1 / Server 2012 R2";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "10.0.10240":
                        result.MarketingCodename = "";
                        result.Name = "Windows 10 1507";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "10.0.10586":
                        result.MarketingCodename = "November Update";
                        result.Name = "Windows 10 1511";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "10.0.14393":
                        result.MarketingCodename = "Anniversary Update";
                        result.Name = "Windows 10 1607";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixCore;
                        break;
                    case "10.0.15063":
                        result.MarketingCodename = "Creators Update";
                        result.Name = "Windows 10 1703";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.16299":
                        result.MarketingCodename = "Fall Creators Update";
                        result.Name = "Windows 10 1709";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.17134":
                        result.MarketingCodename = "April 2018 Update";
                        result.Name = "Windows 10 1803";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.17763":
                        result.MarketingCodename = "October 2018 Update";
                        result.Name = "Windows 10 1809";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.18362":
                        result.MarketingCodename = "May 2019 Update";
                        result.Name = "Windows 10 1903";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.18363":
                        result.MarketingCodename = "November 2019 Update";
                        result.Name = "Windows 10 1909";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.19041":
                        result.MarketingCodename = "May 2020 Update";
                        result.Name = "Windows 10 2004";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.19042":
                        result.MarketingCodename = "October 2020 Update";
                        result.Name = "Windows 10 20H2";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    case "10.0.19043":
                        result.MarketingCodename = "May 2021 Update";
                        result.Name = "Windows 10 21H1";
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        break;
                    default:
                        result.Name = "Windows " + version;
                        result.IsNativeMsixPlatform = AppxTargetOperatingSystemType.MsixNativeSupported;
                        return result;
                }
            }

            return result;
        }
    }
}
