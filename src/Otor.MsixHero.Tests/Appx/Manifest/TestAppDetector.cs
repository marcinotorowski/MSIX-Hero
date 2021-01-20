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
