using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.TimeStamping;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Tests.AppAttach
{
    [Ignore("Integration")]
    [TestFixture(Description = "Various tests for app attach. They call msixmgr.exe and are potentially impacting the system. Admin rights are required to execute them.")]
    [Category("Integration")]
    public class AppAttachIntegrationTests
    {
        [Test]
        [Category("Integration")]
        public void TestSingleNonVhd()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var targetDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "app-attach", "single"));

            var appAttachManager = new AppAttachManager(new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new DummyConfigurationService());
            var vhdxPath = Path.Combine(targetDirectory.FullName, "output.vhdx");
            var cimPath = Path.Combine(targetDirectory.FullName, "output.cim");

            try
            {
                var options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhdx,
                    ExtractCertificate = true,
                    GenerateScripts = true
                };

                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdxPath, 0, options, CancellationToken.None).Wait();

                options.Type = AppAttachVolumeType.Cim;
                appAttachManager.CreateVolume(msixHeroPackage.FullName, cimPath, 0, options, CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }
            
            Assert.IsTrue(File.Exists(vhdxPath));
            Assert.IsTrue(File.Exists(cimPath));
        }

        [Test]
        public void TestSingleVhdWithJsonAndExtraFiles()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var targetDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "app-attach", "scripts-json"));

            var appAttachManager = new AppAttachManager(new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new DummyConfigurationService());
            var vhdPathNothing = Path.Combine(targetDirectory.FullName, "nothing", "output.vhd");
            var vhdPathScripts = Path.Combine(targetDirectory.FullName, "scripts", "output.vhd");
            var vhdPathCertificate = Path.Combine(targetDirectory.FullName, "certificate", "output.vhd");
            var vhdPathBoth = Path.Combine(targetDirectory.FullName, "both", "output.vhd");
            
            try
            {
                var options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhd,
                    GenerateScripts = false,
                    ExtractCertificate = false
                };

                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPathNothing, 0, options, CancellationToken.None).Wait();

                options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhd,
                    GenerateScripts = false,
                    ExtractCertificate = true
                };

                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPathScripts, 0, options, CancellationToken.None).Wait();

                options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhd,
                    GenerateScripts = true,
                    ExtractCertificate = false
                };

                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPathCertificate, 0, options, CancellationToken.None).Wait();

                options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhd,
                    GenerateScripts = true,
                    ExtractCertificate = true
                };

                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPathBoth, 0, options, CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }

            var cerPathNothing = Path.Combine(targetDirectory.FullName, "nothing", "output.cer");
            var cerPathScripts = Path.Combine(targetDirectory.FullName, "scripts", "output.cer");
            var cerPathCertificate = Path.Combine(targetDirectory.FullName, "certificate", "output.cer");
            var cerPathBoth = Path.Combine(targetDirectory.FullName, "both", "output.cer");

            var scriptPathNothing = Path.Combine(targetDirectory.FullName, "nothing", "stage.ps1");
            var scriptPathScripts = Path.Combine(targetDirectory.FullName, "scripts", "stage.ps1");
            var scriptPathCertificate = Path.Combine(targetDirectory.FullName, "certificate", "stage.ps1");
            var scriptPathBoth = Path.Combine(targetDirectory.FullName, "both", "stage.ps1");

            var jsonPathNothing = Path.Combine(targetDirectory.FullName, "nothing", "app-attach.json");
            var jsonPathScripts = Path.Combine(targetDirectory.FullName, "scripts", "app-attach.json");
            var jsonPathCertificate = Path.Combine(targetDirectory.FullName, "certificate", "app-attach.json");
            var jsonPathBoth = Path.Combine(targetDirectory.FullName, "both", "app-attach.json");

            Assert.False(File.Exists(cerPathNothing));
            Assert.False(File.Exists(cerPathScripts));
            Assert.True(File.Exists(cerPathCertificate));
            Assert.True(File.Exists(cerPathBoth));

            Assert.False(File.Exists(scriptPathNothing));
            Assert.True(File.Exists(scriptPathScripts));
            Assert.False(File.Exists(scriptPathCertificate));
            Assert.True(File.Exists(scriptPathBoth));

            Assert.True(File.Exists(jsonPathNothing));
            Assert.True(File.Exists(jsonPathScripts));
            Assert.True(File.Exists(jsonPathCertificate));
            Assert.True(File.Exists(jsonPathBoth));
        }

        [Test]
        public void TestSingleVhdWithCustomSize()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var targetDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "app-attach", "custom-size"));

            var appAttachManager = new AppAttachManager(new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new DummyConfigurationService());
            var vhdPath1 = Path.Combine(targetDirectory.FullName, "output1.vhd");
            var vhdPath2 = Path.Combine(targetDirectory.FullName, "output2.vhd");

            var options = new AppAttachNewVolumeOptions
            {
                Type = AppAttachVolumeType.Vhd,
                ExtractCertificate = true,
                GenerateScripts = true
            };

            try
            {
                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPath1, 0, options, CancellationToken.None).Wait();
                appAttachManager.CreateVolume(msixHeroPackage.FullName, vhdPath2, 50, options, CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }

            Assert.IsTrue(File.Exists(vhdPath1));
            Assert.IsTrue(File.Exists(vhdPath2));

            var fileSize1 = new FileInfo(vhdPath1).Length;
            var fileSize2 = new FileInfo(vhdPath2).Length;
            
            Assert.AreEqual(50, (int)fileSize2 / 1024 / 1024);
            Assert.Greater(fileSize2, fileSize1, "Custom size 50MB must product a bigger package than the auto-size of a relatively small package.");
        }

        [Test]
        public void TestMultipleFiles()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var targetDirectory = Path.Combine(Environment.CurrentDirectory, "app-attach", "multiple");
            var targetVhd = Path.Combine(targetDirectory, "vhd");
            var targetVhdx = Path.Combine(targetDirectory, "vhdx");
            var targetCim = Path.Combine(targetDirectory, "cim");
            
            var baseSourceFolder = Path.Combine(Environment.CurrentDirectory, "copied");
            Directory.CreateDirectory(baseSourceFolder);
            
            var sourceFiles = new[] { "pkg1.msix", "pkg2.msix", "pkg3.msix" }.Select(f => Path.Combine(baseSourceFolder, f)).ToArray();
            foreach (var src in sourceFiles)
            {
                msixHeroPackage.CopyTo(src, true);
            }
            
            var appAttachManager = new AppAttachManager(new SigningManager(MsixHeroGistTimeStampFeed.CreateCached()), new DummyConfigurationService());

            try
            {
                var options = new AppAttachNewVolumeOptions
                {
                    Type = AppAttachVolumeType.Vhd,
                    ExtractCertificate = true,
                    GenerateScripts = true
                };

                appAttachManager.CreateVolumes(sourceFiles, targetVhd, options, CancellationToken.None).Wait();

                options.Type = AppAttachVolumeType.Vhdx;
                appAttachManager.CreateVolumes(sourceFiles, targetVhdx, options, CancellationToken.None).Wait();

                options.Type = AppAttachVolumeType.Cim;
                appAttachManager.CreateVolumes(sourceFiles, targetCim, options, CancellationToken.None).Wait();
            }
            catch (AggregateException e)
            {
                throw e.GetBaseException();
            }

            Assert.True(File.Exists(Path.Combine(targetVhd, "pkg1.vhd")));
            Assert.True(File.Exists(Path.Combine(targetVhd, "pkg2.vhd")));
            Assert.True(File.Exists(Path.Combine(targetVhd, "pkg3.vhd")));

            Assert.True(File.Exists(Path.Combine(targetVhdx, "pkg1.vhdx")));
            Assert.True(File.Exists(Path.Combine(targetVhdx, "pkg2.vhdx")));
            Assert.True(File.Exists(Path.Combine(targetVhdx, "pkg3.vhdx")));
            
            Assert.True(Directory.Exists(Path.Combine(targetCim, "pkg1")));
            Assert.True(Directory.Exists(Path.Combine(targetCim, "pkg2")));
            Assert.True(Directory.Exists(Path.Combine(targetCim, "pkg3")));
        }
    }

    public class DummyConfigurationService : IConfigurationService
    {
        private Configuration configuration = new Configuration();

        public Task<Configuration> GetCurrentConfigurationAsync(bool preferCached = true, CancellationToken token = default)
        {
            return Task.FromResult(this.configuration);
        }

        public Configuration GetCurrentConfiguration(bool preferCached = true)
        {
            return this.configuration;
        }

        public Task SetCurrentConfigurationAsync(Configuration cfg, CancellationToken cancellationToken = default)
        {
            this.configuration = cfg;
            return Task.CompletedTask;
        }

        public void SetCurrentConfiguration(Configuration cfg)
        {
            this.configuration = cfg;
        }
    }
}
