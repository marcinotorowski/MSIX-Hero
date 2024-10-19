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
using NUnit.Framework;
using Otor.MsixHero.Appx.Diagnostic.System;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Tests.Appx.Diagnostics
{
    [TestFixture]
    public class WindowsNameTests
    {
        [Test]
        public void TestMsixCore()
        {
            var windows7 = new Tuple<string, string>("Windows.Desktop", "6.1.7601");
            var windowsServer2012 = new Tuple<string, string>("Windows.Desktop", "6.2.9200");
            var windows81 = new Tuple<string, string>("Windows.Desktop", "6.3.9600");
            var windows101507 = new Tuple<string, string>("Windows.Desktop", "10.0.10240");
            var windows101511 = new Tuple<string, string>("Windows.Desktop", "10.0.10586");
            var windows101607 = new Tuple<string, string>("Windows.Desktop", "10.0.14393");
            var windows101703 = new Tuple<string, string>("Windows.Desktop", "10.0.15063");
            var windows101709 = new Tuple<string, string>("Windows.Desktop", "10.0.16299");
            var windows101803 = new Tuple<string, string>("Windows.Desktop", "10.0.17134");
            var windows101809 = new Tuple<string, string>("Windows.Desktop", "10.0.17763");
            var windows101903 = new Tuple<string, string>("Windows.Desktop", "10.0.18362");
            var windows101909 = new Tuple<string, string>("Windows.Desktop", "10.0.18363");
            var windows102004 = new Tuple<string, string>("Windows.Desktop", "10.0.19041");
            var windows1020H2 = new Tuple<string, string>("Windows.Desktop", "10.0.19042");
            var windows1021H1 = new Tuple<string, string>("Windows.Desktop", "10.0.19043");
            var windows1121H2 = new Tuple<string, string>("Windows.Desktop", "10.0.22000");
            var windows1122H2 = new Tuple<string, string>("Windows.Desktop", "10.0.22621");
            var windows1123H2 = new Tuple<string, string>("Windows.Desktop", "10.0.22631");
            var windows1124H2 = new Tuple<string, string>("Windows.Desktop", "10.0.26100");
            var windowsOther = new Tuple<string, string>("Windows.Desktop", "10.0.99999");

            var parsedWindows7 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows7.Item1, windows7.Item2);
            var parsedWindowsServer2012 = WindowsNames.GetOperatingSystemFromNameAndVersion(windowsServer2012.Item1, windowsServer2012.Item2);
            var parsedWindows81 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows81.Item1, windows81.Item2);
            var parsedWindows101507 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101507.Item1, windows101507.Item2);
            var parsedWindows101511 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101511.Item1, windows101511.Item2);
            var parsedWindows101607 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101607.Item1, windows101607.Item2);
            var parsedWindows101703 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101703.Item1, windows101703.Item2);
            var parsedWindows101709 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101709.Item1, windows101709.Item2);
            var parsedWindows101803 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101803.Item1, windows101803.Item2);
            var parsedWindows101809 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101809.Item1, windows101809.Item2);
            var parsedWindows101903 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101903.Item1, windows101903.Item2);
            var parsedWindows101909 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows101909.Item1, windows101909.Item2);
            var parsedWindows102004 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows102004.Item1, windows102004.Item2);
            var parsedWindows1020H2 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1020H2.Item1, windows1020H2.Item2);
            var parsedWindows1021H1 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1021H1.Item1, windows1021H1.Item2);
            var parsedWindows1121H2 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1121H2.Item1, windows1121H2.Item2);
            var parsedWindows1122H2 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1122H2.Item1, windows1122H2.Item2);
            var parsedWindows1123H2 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1123H2.Item1, windows1123H2.Item2);
            var parsedWindows1124H2 = WindowsNames.GetOperatingSystemFromNameAndVersion(windows1124H2.Item1, windows1124H2.Item2);
            var parsedWindowsOther = WindowsNames.GetOperatingSystemFromNameAndVersion(windowsOther.Item1, windowsOther.Item2);
            
            // Test if technical versions were recognized
            Assert.That(parsedWindows7.TechnicalVersion, Is.EqualTo("6.1.7601"));
            Assert.That(parsedWindowsServer2012.TechnicalVersion, Is.EqualTo("6.2.9200"));
            Assert.That(parsedWindows81.TechnicalVersion, Is.EqualTo("6.3.9600"));
            Assert.That(parsedWindows101507.TechnicalVersion, Is.EqualTo("10.0.10240"));
            Assert.That(parsedWindows101511.TechnicalVersion, Is.EqualTo("10.0.10586"));
            Assert.That(parsedWindows101607.TechnicalVersion, Is.EqualTo("10.0.14393"));
            Assert.That(parsedWindows101703.TechnicalVersion, Is.EqualTo("10.0.15063"));
            Assert.That(parsedWindows101709.TechnicalVersion, Is.EqualTo("10.0.16299"));
            Assert.That(parsedWindows101803.TechnicalVersion, Is.EqualTo("10.0.17134"));
            Assert.That(parsedWindows101809.TechnicalVersion, Is.EqualTo("10.0.17763"));
            Assert.That(parsedWindows101903.TechnicalVersion, Is.EqualTo("10.0.18362"));
            Assert.That(parsedWindows101909.TechnicalVersion, Is.EqualTo("10.0.18363"));
            Assert.That(parsedWindows102004.TechnicalVersion, Is.EqualTo("10.0.19041"));
            Assert.That(parsedWindows1020H2.TechnicalVersion, Is.EqualTo("10.0.19042"));
            Assert.That(parsedWindows1021H1.TechnicalVersion, Is.EqualTo("10.0.19043"));
            Assert.That(parsedWindows1121H2.TechnicalVersion, Is.EqualTo("10.0.22000"));
            Assert.That(parsedWindows1122H2.TechnicalVersion, Is.EqualTo("10.0.22621"));
            Assert.That(parsedWindows1123H2.TechnicalVersion, Is.EqualTo("10.0.22631"));
            Assert.That(parsedWindows1124H2.TechnicalVersion, Is.EqualTo("10.0.26100"));
            Assert.That(parsedWindowsOther.TechnicalVersion, Is.EqualTo("10.0.99999"));

            // Test if display versions were recognized
            Assert.That(parsedWindows7.Name, Is.EqualTo("Windows 7 SP1 / Server 2008 R2"));
            Assert.That(parsedWindowsServer2012.Name, Is.EqualTo("Windows 8 / Server 2012"));
            Assert.That(parsedWindows81.Name, Is.EqualTo("Windows 8.1 / Server 2012 R2"));
            Assert.That(parsedWindows101507.Name, Is.EqualTo("Windows 10 1507"));
            Assert.That(parsedWindows101511.Name, Is.EqualTo("Windows 10 1511"));
            Assert.That(parsedWindows101607.Name, Is.EqualTo("Windows 10 1607"));
            Assert.That(parsedWindows101703.Name, Is.EqualTo("Windows 10 1703"));
            Assert.That(parsedWindows101709.Name, Is.EqualTo("Windows 10 1709"));
            Assert.That(parsedWindows101803.Name, Is.EqualTo("Windows 10 1803"));
            Assert.That(parsedWindows101809.Name, Is.EqualTo("Windows 10 1809"));
            Assert.That(parsedWindows101903.Name, Is.EqualTo("Windows 10 1903"));
            Assert.That(parsedWindows101909.Name, Is.EqualTo("Windows 10 1909"));
            Assert.That(parsedWindows102004.Name, Is.EqualTo("Windows 10 2004"));
            Assert.That(parsedWindows1020H2.Name, Is.EqualTo("Windows 10 20H2"));
            Assert.That(parsedWindows1021H1.Name, Is.EqualTo("Windows 10 21H1"));
            Assert.That(parsedWindows1121H2.Name, Is.EqualTo("Windows 11 21H2"));
            Assert.That(parsedWindows1122H2.Name, Is.EqualTo("Windows 11 22H2"));
            Assert.That(parsedWindows1123H2.Name, Is.EqualTo("Windows 11 23H2"));
            Assert.That(parsedWindows1124H2.Name, Is.EqualTo("Windows 11 24H2"));
            Assert.That(parsedWindowsOther.Name, Is.EqualTo("Windows 11 Build 99999"));

            // Test if type of support for MSIX was recognized
            Assert.That(parsedWindows7.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            Assert.That(parsedWindowsServer2012.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            Assert.That(parsedWindows81.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            Assert.That(parsedWindows101507.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            Assert.That(parsedWindows101511.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            Assert.That(parsedWindows101607.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixCore));
            
            Assert.That(parsedWindows101703.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows101709.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows101803.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows101809.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows101903.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows101909.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows102004.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1020H2.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1021H1.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1121H2.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1122H2.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1123H2.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));
            Assert.That(parsedWindows1124H2.IsNativeMsixPlatform, Is.EqualTo(AppxTargetOperatingSystemType.MsixNativeSupported));

            // Test if marketing names were recognized
            Assert.That(parsedWindows101507.MarketingCodename, Is.EqualTo("RTM"));
            Assert.That(parsedWindows101511.MarketingCodename, Is.EqualTo("November Update"));
            Assert.That(parsedWindows101607.MarketingCodename, Is.EqualTo("Anniversary Update"));
            Assert.That(parsedWindows101703.MarketingCodename, Is.EqualTo("Creators Update"));
            Assert.That(parsedWindows101709.MarketingCodename, Is.EqualTo("Fall Creators Update")); 
            Assert.That(parsedWindows101803.MarketingCodename, Is.EqualTo("April 2018 Update"));
            Assert.That(parsedWindows101809.MarketingCodename, Is.EqualTo("October 2018 Update"));
            Assert.That(parsedWindows101903.MarketingCodename, Is.EqualTo("May 2019 Update"));
            Assert.That(parsedWindows101909.MarketingCodename, Is.EqualTo("November 2019 Update"));
            Assert.That(parsedWindows102004.MarketingCodename, Is.EqualTo("May 2020 Update"));
            Assert.That(parsedWindows1020H2.MarketingCodename, Is.EqualTo("October 2020 Update"));
            Assert.That(parsedWindows1021H1.MarketingCodename, Is.EqualTo("May 2021 Update"));
        }
    }
}
