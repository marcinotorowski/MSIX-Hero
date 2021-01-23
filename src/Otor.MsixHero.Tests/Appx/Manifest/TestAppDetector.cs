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
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.Manifest.Helpers;

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
                
                Assert.IsTrue(packagingToolDetector.TryDetectMsixHero(buildParams, out _));
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

            Assert.IsTrue(packagingToolDetector.TryDetectRayPack(buildParams, out var rpk));
            Assert.AreEqual("RayPack 6.2", rpk.ProductName);
            Assert.AreEqual("(MSIX builder v6.2.7060.150)", rpk.ProductVersion);
        }
    }
}
