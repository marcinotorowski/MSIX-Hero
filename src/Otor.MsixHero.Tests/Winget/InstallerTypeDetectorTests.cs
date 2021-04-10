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
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Winget.Helpers;
using Otor.MsixHero.Winget.Yaml.Entities;

namespace Otor.MsixHero.Tests.Winget
{
    [TestFixture]
    public class InstallerTypeDetectorTests
    {
        [Test]
        public void TestNsis()
        {
            var sd = new InstallerTypeDetector();

            var resources = typeof(InstallerTypeDetectorTests).Assembly.GetManifestResourceNames();

            var nsis = resources.First(r => r.IndexOf("nsis", StringComparison.OrdinalIgnoreCase) != -1);

            using (var s = typeof(InstallerTypeDetectorTests).Assembly.GetManifestResourceStream(nsis))
            {
                var detect = sd.DetectSetupType(s).Result;
                Assert.AreEqual(YamlInstallerType.Nullsoft, detect);
            }
        }

        [Test]
        public void TestInno()
        {
            var sd = new InstallerTypeDetector();

            var resources = typeof(InstallerTypeDetectorTests).Assembly.GetManifestResourceNames();
            var inno = resources.First(r => r.IndexOf("inno", StringComparison.OrdinalIgnoreCase) != -1);
            using (var s = typeof(InstallerTypeDetectorTests).Assembly.GetManifestResourceStream(inno))
            {
                var detect = sd.DetectSetupType(s).Result;
                Assert.AreEqual(YamlInstallerType.InnoSetup, detect);
            }
        }
    }
}
