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

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest.Helpers;

namespace Otor.MsixHero.Tests.Appx.Manifest
{
    [TestFixture]
    public class TestAppDetector
    {
        [Test]
        public void TestMsixHeroDetection()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));

            using (var reader = FileReaderFactory.CreateFileReader(msixHeroPackage.FullName))
            {
                var packagingToolDetector = new AuthoringAppDetector(reader);
                
                var buildParams = new Dictionary<string, string>
                {
                    { "MsixHero", "1.5.0" }
                };
                
                Assert.That(packagingToolDetector.TryDetectMsixHero(buildParams, out _), Is.True);
            }
        }
        
        [Test]
        public void TestNewRayPackDetection()
        {
            var packagingToolDetector = new AuthoringAppDetector(null);

            var buildParams = new Dictionary<string, string>
            {
                { "Raynet.RaySuite.Common.Appx", "6.2.7060.150"},
            };

            Assert.That(packagingToolDetector.TryDetectRayPack(buildParams, out var rpk), Is.True);
            Assert.That(rpk.ProductName, Is.EqualTo("RayPack 6.2"));
            Assert.That(rpk.ProductVersion, Is.EqualTo("(MSIX builder v6.2.7060.150)"));
        }
    }
}
