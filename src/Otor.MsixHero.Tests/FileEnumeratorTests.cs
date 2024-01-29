using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Tests
{
    public class FileEnumeratorTests
    {
        [Test]
        public async Task TestRootItems()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var reader = FileReaderFactory.CreateFileReader(msixHeroPackage.FullName);

            var folders = new List<string>();
            var files = new List<string>();

            await foreach (var dir in reader.EnumerateDirectories())
            {
                folders.Add(dir);
            }

            await foreach (var file in reader.EnumerateFiles())
            {
                files.Add(file.Name);
            }
            
            Assert.That(folders.Contains("VFS"), Is.True);
            Assert.That(folders.Contains("Assets"), Is.True);

            Assert.That(files.Contains("AppxManifest.xml"), Is.True);
        }

        [Test]
        public async Task TestWildcard()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var reader = FileReaderFactory.CreateFileReader(msixHeroPackage.FullName);
            
            var files = new List<AppxFileInfo>();
            
            await foreach (var file in reader.EnumerateFiles(@"VFS\AppVPackageDrive\ConEmuPack", "psf*.exe"))
            {
                files.Add(file);
            }

            Assert.That(files.Single().FullPath, Is.EqualTo(@"VFS\AppVPackageDrive\ConEmuPack\PsfLauncher1.exe"));
        }

        [Test]
        public async Task TestSubitems()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var reader = FileReaderFactory.CreateFileReader(msixHeroPackage.FullName);

            var folders = await reader.EnumerateDirectories(@"VFS\AppVPackageDrive").ToListAsync();
            var files = await reader.EnumerateFiles(@"VFS\AppVPackageDrive\ConEmuPack").ToListAsync();

            Assert.That(new[] { @"VFS\AppVPackageDrive\ConEmuPack" }.OrderBy(c => c).SequenceEqual(folders.OrderBy(d => d)), Is.True);
            Assert.That(new[] { @"VFS\AppVPackageDrive\ConEmuPack\ConEmu.exe", @"VFS\AppVPackageDrive\ConEmuPack\ConEmu64.exe", @"VFS\AppVPackageDrive\ConEmuPack\PsfLauncher1.exe" }.OrderBy(c => c).SequenceEqual(files.Select(f => f.FullPath).OrderBy(f => f)), Is.True);
        }



    }
}
