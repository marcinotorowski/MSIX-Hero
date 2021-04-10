using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class IdentityReaderTests
    {
        [Test]
        public void TestInvalidXmlFile()
        {
            var invalidContent =
            @"<?xml version=""1.0""?>
<NotSupportedRoot xmlns=""http://schemas.microsoft.com/appx/manifest/foundation/windows10"">
  <Identity 
    Name=""SampleProduct"" 
    Publisher=""CN=me"" 
    Version=""1.2.3.4"" 
    ProcessorArchitecture=""x64"" />
</NotSupportedRoot>
";

            var utfEncoding = new UTF8Encoding(false, false);
            var xmlStream = new MemoryStream(utfEncoding.GetBytes(invalidContent));

            IAppxIdentityReader reader = new AppxIdentityReader();

            Assert.Throws<ArgumentException>(() =>
            {
                GuardAggregateExceptions(() =>
                {
                    reader.GetIdentity(xmlStream).Wait();
                });
            }, 
            "This must throw because the file is definitely not in MSIX, APPX or any other supported format.");
        }
        
        [Test]
        public void TestInvalidZipFile()
        {
            using var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // add some dummy file = not a manifest
                var dummyFile = zipArchive.CreateEntry("dummyFile.xml");
                var s = dummyFile.Open();
                s.WriteByte(128);
            }
            
            IAppxIdentityReader reader = new AppxIdentityReader();

            Assert.Throws<ArgumentException>(() =>
            {
                GuardAggregateExceptions(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    reader.GetIdentity(memoryStream).Wait();
                });
            }, 
            "This must throw because the file is a ZIP-like but contains no manifest.");
        }

        [Test]
        public void TestAppxBundleManifest()
        {
            // in a manifest, from memory
            GuardAggregateExceptions(() =>
            {
                IAppxIdentityReader reader = new AppxIdentityReader();

                var embeddedResourceName = this.GetType().Assembly.GetManifestResourceNames().First(rn => rn.Contains("resources.appxbundlemanifest.xml", StringComparison.OrdinalIgnoreCase));
                using var bundleManifest = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);
                
                var identity = reader.GetIdentity(bundleManifest).Result;

                Assert.AreEqual("Microsoft.DesktopAppInstaller", identity.Name);
                Assert.AreEqual("CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US", identity.Publisher);
                Assert.AreEqual("2021.119.2316.0", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(3, identity.Architectures.Length);
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.Arm));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x64));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x86));
            });

            // in a manifest, saved on disk
            GuardTempDirectory(tempDirectory =>
            {
                IAppxIdentityReader reader = new AppxIdentityReader();

                var manifestFile = Path.Combine(tempDirectory.FullName, FileConstants.AppxBundleManifestFile);

                using (var fs = File.OpenWrite(manifestFile))
                {
                    var embeddedResourceName = this.GetType().Assembly.GetManifestResourceNames().First(rn => rn.Contains("resources.appxbundlemanifest.xml", StringComparison.OrdinalIgnoreCase));
                    using var bundleManifest = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);

                    // ReSharper disable once PossibleNullReferenceException
                    bundleManifest.CopyTo(fs);
                    fs.Flush();
                }

                var identity = reader.GetIdentity(manifestFile).Result;
                Assert.AreEqual("Microsoft.DesktopAppInstaller", identity.Name);
                Assert.AreEqual("CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US", identity.Publisher);
                Assert.AreEqual("2021.119.2316.0", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(3, identity.Architectures.Length);
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.Arm));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x64));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x86));
            });

            // in a zip file
            GuardAggregateExceptions(() =>
            {
                using var memStream = new MemoryStream();

                // build package resembling MSIX structure
                using (var stream = new ZipArchive(memStream, ZipArchiveMode.Create, true))
                {
                    var entry = stream.CreateEntry(FileConstants.AppxBundleManifestFilePath);
                    var entryStream = entry.Open();

                    var embeddedResourceName = this.GetType().Assembly.GetManifestResourceNames().First(rn => rn.Contains("resources.appxbundlemanifest.xml", StringComparison.OrdinalIgnoreCase));
                    using var bundleManifest = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);

                    // ReSharper disable once PossibleNullReferenceException
                    bundleManifest.CopyTo(entryStream);
                }

                memStream.Seek(0, SeekOrigin.Begin);
                IAppxIdentityReader reader = new AppxIdentityReader();
                var identity = reader.GetIdentity(memStream).Result;
                Assert.AreEqual("Microsoft.DesktopAppInstaller", identity.Name);
                Assert.AreEqual("CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US", identity.Publisher);
                Assert.AreEqual("2021.119.2316.0", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(3, identity.Architectures.Length);
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.Arm));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x64));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x86));
            });

            // in a zip file, saved on disk
            GuardTempDirectory(tempDirectory =>
            {
                var tempFile = Path.Combine(tempDirectory.FullName, "testpackage.appxbundle");

                using (var tempFileStream = File.OpenWrite(tempFile))
                {
                    // build package resembling MSIX structure
                    using var stream = new ZipArchive(tempFileStream, ZipArchiveMode.Create, false);
                    var entry = stream.CreateEntry(FileConstants.AppxBundleManifestFilePath);
                    var entryStream = entry.Open();

                    var embeddedResourceName = this.GetType().Assembly.GetManifestResourceNames().First(rn => rn.Contains("resources.appxbundlemanifest.xml", StringComparison.OrdinalIgnoreCase));
                    using var bundleManifest = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);

                    // ReSharper disable once PossibleNullReferenceException
                    bundleManifest.CopyTo(entryStream);
                }

                IAppxIdentityReader reader = new AppxIdentityReader();
                var identity = reader.GetIdentity(tempFile).Result;
                Assert.AreEqual("Microsoft.DesktopAppInstaller", identity.Name);
                Assert.AreEqual("CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US", identity.Publisher);
                Assert.AreEqual("2021.119.2316.0", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(3, identity.Architectures.Length);
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.Arm));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x64));
                Assert.IsTrue(identity.Architectures.Contains(AppxPackageArchitecture.x86));
            });
        }

        [Test]
        public void TestMismatchedExtension()
        {
            GuardTempDirectory(tempDirectory =>
            {
                // Build an APPX package which is in reality an appx bundle.
                var tempFile = Path.Combine(tempDirectory.FullName, "bundle.appx");
                using (var fileStream = File.OpenWrite(tempFile))
                {
                    using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    {
                        var entry = zip.CreateEntry(FileConstants.AppxBundleManifestFilePath);
                        var entryStream = entry.Open();

                        var embeddedResourceName = this.GetType().Assembly.GetManifestResourceNames().First(rn => rn.Contains("resources.appxbundlemanifest.xml", StringComparison.OrdinalIgnoreCase));
                        using var bundleManifest = this.GetType().Assembly.GetManifestResourceStream(embeddedResourceName);

                        // ReSharper disable once PossibleNullReferenceException
                        bundleManifest.CopyTo(entryStream);
                    }
                }

                IAppxIdentityReader reader = new AppxIdentityReader();
                Assert.Throws<ArgumentException>(() =>
                {
                    GuardAggregateExceptions(() =>
                    {
                        reader.GetIdentity(tempFile).Wait();
                    });
                },
                "This must throw, because the file on a disk has an APPX extension but the content is a bundle.");

                using (var fileStream = File.OpenRead(tempFile))
                {
                    Assert.Throws<ArgumentException>(() =>
                    {
                        GuardAggregateExceptions(() =>
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            reader.GetIdentity(fileStream).Wait();
                        });
                    }, 
                    "This must throw, because file stream points to a concrete file on a disk which has an APPX extension but the content is a bundle.");
                }

                using var memoryStream = new MemoryStream(File.ReadAllBytes(tempFile));
                // The following may not throw, because a memory stream will be tried for various options and asserted to be an APPX bundle.
                reader.GetIdentity(memoryStream).Wait();
            });
        }
        
        [Test]
        public void TestAppxManifest()
        {
            // in a manifest, from memory
            GuardAggregateExceptions(() =>
            {
                IAppxIdentityReader reader = new AppxIdentityReader();

                var utfEncoding = new UTF8Encoding(false, false);
                
                var stream = new MemoryStream(utfEncoding.GetBytes(SimpleAppxManifestContent));
                var identity = reader.GetIdentity(stream).Result;
                Assert.AreEqual("SampleProduct", identity.Name);
                Assert.AreEqual("CN=me", identity.Publisher);
                Assert.AreEqual("1.2.3.4", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(1, identity.Architectures.Length);
                Assert.AreEqual(AppxPackageArchitecture.x64, identity.Architectures[0]);
            });

            // in a manifest, saved on disk
            GuardTempDirectory(tempDirectory =>
            {
                IAppxIdentityReader reader = new AppxIdentityReader();

                var manifestFile = Path.Combine(tempDirectory.FullName, FileConstants.AppxManifestFile);
                var utfEncoding = new UTF8Encoding(false, false);
                File.WriteAllText(manifestFile, SimpleAppxManifestContent, utfEncoding);

                var identity = reader.GetIdentity(manifestFile).Result;
                Assert.AreEqual("SampleProduct", identity.Name);
                Assert.AreEqual("CN=me", identity.Publisher);
                Assert.AreEqual("1.2.3.4", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(1, identity.Architectures.Length);
                Assert.AreEqual(AppxPackageArchitecture.x64, identity.Architectures[0]);
            });

            // in a zip file
            GuardAggregateExceptions(() =>
            {
                using var memStream = new MemoryStream();

                // build package resembling MSIX structure
                using (var stream = new ZipArchive(memStream, ZipArchiveMode.Create, true))
                {
                    var entry = stream.CreateEntry(FileConstants.AppxManifestFile);
                    var entryStream = entry.Open();
                    var utfEncoding = new UTF8Encoding(false, false);
                    using (var streamWriter = new StreamWriter(entryStream, utfEncoding, leaveOpen: true))
                    {
                        streamWriter.Write(SimpleAppxManifestContent);
                    }
                }

                memStream.Seek(0, SeekOrigin.Begin);
                IAppxIdentityReader reader = new AppxIdentityReader();
                var identity = reader.GetIdentity(memStream).Result;
                Assert.AreEqual("SampleProduct", identity.Name);
                Assert.AreEqual("CN=me", identity.Publisher);
                Assert.AreEqual("1.2.3.4", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(1, identity.Architectures.Length);
                Assert.AreEqual(AppxPackageArchitecture.x64, identity.Architectures[0]);
            });

            // in a zip file, saved on disk
            GuardTempDirectory(tempDirectory =>
            {
                var tempFile = Path.Combine(tempDirectory.FullName, "testpackage.appx");
                
                using (var tempFileStream = File.OpenWrite(tempFile))
                {
                    // build package resembling MSIX structure
                    using (var stream = new ZipArchive(tempFileStream, ZipArchiveMode.Create, false))
                    {
                        var entry = stream.CreateEntry(FileConstants.AppxManifestFile);
                        var entryStream = entry.Open();
                        var utfEncoding = new UTF8Encoding(false, false);
                        using (var streamWriter = new StreamWriter(entryStream, utfEncoding, leaveOpen: true))
                        {
                            streamWriter.Write(SimpleAppxManifestContent);
                        }
                    }
                }

                IAppxIdentityReader reader = new AppxIdentityReader();
                var identity = reader.GetIdentity(tempFile).Result;
                Assert.AreEqual("SampleProduct", identity.Name);
                Assert.AreEqual("CN=me", identity.Publisher);
                Assert.AreEqual("1.2.3.4", identity.Version);
                Assert.NotNull(identity.Architectures);
                Assert.AreEqual(1, identity.Architectures.Length);
                Assert.AreEqual(AppxPackageArchitecture.x64, identity.Architectures[0]);
            });
        }
        
        private const string SimpleAppxManifestContent =
        @"<?xml version=""1.0""?>
<Package xmlns=""http://schemas.microsoft.com/appx/manifest/foundation/windows10"">
  <Identity 
    Name=""SampleProduct"" 
    Publisher=""CN=me"" 
    Version=""1.2.3.4"" 
    ProcessorArchitecture=""x64"" />
</Package>
";

        private static void GuardTempDirectory(Action<DirectoryInfo> toExecute)
        {
            GuardAggregateExceptions(() =>
            {
                var tempDir = new DirectoryInfo(Guid.NewGuid().ToString("N"));
                tempDir.Create();
                try
                {
                    toExecute(tempDir);
                }
                finally
                {
                    tempDir.Delete(true);
                }
            });
        }

        private static void GuardAggregateExceptions(Action toExecute)
        {
            try
            {
                toExecute();
            }
            catch (AggregateException exception)
            {
                throw exception.GetBaseException();
            }
        }
    }
}
