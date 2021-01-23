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
using NUnit.Framework;
using Otor.MsixHero.Appx.Diagnostic.System;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Tests.Appx.Diagnostics
{
    [TestFixture]
    public class Windows10NameParser
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
            var windowsOther = new Tuple<string, string>("Windows.Desktop", "10.0.99999");

            
            var parsedWindows7 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows7.Item1, windows7.Item2);
            var parsedWindowsServer2012 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windowsServer2012.Item1, windowsServer2012.Item2);
            var parsedWindows81 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows81.Item1, windows81.Item2);
            var parsedWindows101507 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101507.Item1, windows101507.Item2);
            var parsedWindows101511 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101511.Item1, windows101511.Item2);
            var parsedWindows101607 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101607.Item1, windows101607.Item2);
            var parsedWindows101703 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101703.Item1, windows101703.Item2);
            var parsedWindows101709 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101709.Item1, windows101709.Item2);
            var parsedWindows101803 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101803.Item1, windows101803.Item2);
            var parsedWindows101809 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101809.Item1, windows101809.Item2);
            var parsedWindows101903 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101903.Item1, windows101903.Item2);
            var parsedWindows101909 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows101909.Item1, windows101909.Item2);
            var parsedWindows102004 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows102004.Item1, windows102004.Item2);
            var parsedWindows1020H2 = Windows10Parser.GetOperatingSystemFromNameAndVersion(windows1020H2.Item1, windows1020H2.Item2);
            var parsedWindowsOther = Windows10Parser.GetOperatingSystemFromNameAndVersion(windowsOther.Item1, windowsOther.Item2);

            
            // Test if technical versions were recognized
            Assert.AreEqual("6.1.7601", parsedWindows7.TechnicalVersion);
            Assert.AreEqual("6.2.9200", parsedWindowsServer2012.TechnicalVersion);
            Assert.AreEqual("6.3.9600", parsedWindows81.TechnicalVersion);
            Assert.AreEqual("10.0.10240", parsedWindows101507.TechnicalVersion);
            Assert.AreEqual("10.0.10586", parsedWindows101511.TechnicalVersion);
            Assert.AreEqual("10.0.14393", parsedWindows101607.TechnicalVersion);
            Assert.AreEqual("10.0.15063", parsedWindows101703.TechnicalVersion);
            Assert.AreEqual("10.0.16299", parsedWindows101709.TechnicalVersion);
            Assert.AreEqual("10.0.17134", parsedWindows101803.TechnicalVersion);
            Assert.AreEqual("10.0.17763", parsedWindows101809.TechnicalVersion);
            Assert.AreEqual("10.0.18362", parsedWindows101903.TechnicalVersion);
            Assert.AreEqual("10.0.18363", parsedWindows101909.TechnicalVersion);
            Assert.AreEqual("10.0.19041", parsedWindows102004.TechnicalVersion);
            Assert.AreEqual("10.0.19042", parsedWindows1020H2.TechnicalVersion);
            Assert.AreEqual("10.0.99999", parsedWindowsOther.TechnicalVersion);

            // Test if display versions were recognized
            Assert.AreEqual("Windows 7 SP1 / Server 2008 R2", parsedWindows7.Name);
            Assert.AreEqual("Windows Server 2012", parsedWindowsServer2012.Name);
            Assert.AreEqual("Windows 8.1 / Server 2012 R2", parsedWindows81.Name);
            Assert.AreEqual("Windows 10 1507", parsedWindows101507.Name);
            Assert.AreEqual("Windows 10 1511", parsedWindows101511.Name);
            Assert.AreEqual("Windows 10 1607", parsedWindows101607.Name);
            Assert.AreEqual("Windows 10 1703", parsedWindows101703.Name);
            Assert.AreEqual("Windows 10 1709", parsedWindows101709.Name);
            Assert.AreEqual("Windows 10 1803", parsedWindows101803.Name);
            Assert.AreEqual("Windows 10 1809", parsedWindows101809.Name);
            Assert.AreEqual("Windows 10 1903", parsedWindows101903.Name);
            Assert.AreEqual("Windows 10 1909", parsedWindows101909.Name);
            Assert.AreEqual("Windows 10 2004", parsedWindows102004.Name);
            Assert.AreEqual("Windows 10 2004", parsedWindows1020H2.Name);
            Assert.AreEqual("Windows 10.0.99999", parsedWindowsOther.Name);

            // Test if type of support for MSIX was recognized
            Assert.True(parsedWindows7.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            Assert.True(parsedWindowsServer2012.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            Assert.True(parsedWindows81.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            Assert.True(parsedWindows101507.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            Assert.True(parsedWindows101511.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            Assert.True(parsedWindows101607.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixCore);
            
            Assert.True(parsedWindows101703.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows101709.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows101803.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows101809.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows101903.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows101909.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows102004.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);
            Assert.True(parsedWindows1020H2.IsNativeMsixPlatform == AppxTargetOperatingSystemType.MsixNativeSupported);

            // Test if marketing names were recognized
            Assert.AreEqual("November Update", parsedWindows101511.MarketingCodename);
            Assert.AreEqual("Anniversary Update", parsedWindows101607.MarketingCodename);
            Assert.AreEqual("Creators Update", parsedWindows101703.MarketingCodename);
            Assert.AreEqual("Fall Creators Update", parsedWindows101709.MarketingCodename);
            Assert.AreEqual("April 2018 Update", parsedWindows101803.MarketingCodename);
            Assert.AreEqual("October 2018 Update", parsedWindows101809.MarketingCodename);
            Assert.AreEqual("May 2019 Update", parsedWindows101903.MarketingCodename);
            Assert.AreEqual("November 2019 Update", parsedWindows101909.MarketingCodename);
            Assert.AreEqual("May 2020 Update", parsedWindows102004.MarketingCodename);
            Assert.AreEqual("October 2020 Update", parsedWindows1020H2.MarketingCodename);

        }
    }
}
