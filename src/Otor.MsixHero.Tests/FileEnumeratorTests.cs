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
            var files = new List<AppxFileInfo>();

            await foreach (var dir in reader.EnumerateDirectories())
            {
                folders.Add(dir);
            }

            await foreach (var file in reader.EnumerateFiles())
            {
                files.Add(file);
            }
            
            Assert.IsTrue(new[] { "Assets", "VFS" }.OrderBy(c => c).SequenceEqual(folders.OrderBy(d => d)));
            Assert.IsTrue(new[] { "[Content_Types].xml", "AppxBlockMap.xml", "AppxManifest.xml", "config.json", "FileRedirectionFixup64.dll", "PsfLauncher64.exe", "PsfRunDll64.exe", "PsfRuntime64.dll", "Registry.dat", "Resources.pri", "TraceFixup64.dll", "User.dat", "UserClasses.dat" ,"WaitForDebuggerFixup64.dll" }.OrderBy(c => c).SequenceEqual(files.Select(f => f.FullPath).OrderBy(f => f)));
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

            Assert.AreEqual(@"VFS\AppVPackageDrive\ConEmuPack\PsfLauncher1.exe", files.Single().FullPath);
        }

        [Test]
        public async Task TestSubitems()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var reader = FileReaderFactory.CreateFileReader(msixHeroPackage.FullName);

            var folders = new List<string>();
            var files = new List<AppxFileInfo>();

            await foreach (var dir in reader.EnumerateDirectories(@"VFS\AppVPackageDrive"))
            {
                folders.Add(dir);
            }

            await foreach (var file in reader.EnumerateFiles(@"VFS\AppVPackageDrive\ConEmuPack"))
            {
                files.Add(file);
            }

            Assert.IsTrue(new[] { @"VFS\AppVPackageDrive\ConEmuPack" }.OrderBy(c => c).SequenceEqual(folders.OrderBy(d => d)));
            Assert.IsTrue(new[] { @"VFS\AppVPackageDrive\ConEmuPack\ConEmu.exe", @"VFS\AppVPackageDrive\ConEmuPack\ConEmu64.exe", @"VFS\AppVPackageDrive\ConEmuPack\PsfLauncher1.exe" }.OrderBy(c => c).SequenceEqual(files.Select(f => f.FullPath).OrderBy(f => f)));
        }



    }
}
