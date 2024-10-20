using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.File.Adapters;
using Otor.MsixHero.Appx.Reader.Psf;

namespace Otor.MsixHero.Tests.Psf.MsixHelper
{
    [TestFixture]
    internal class MSIXHelperParsingTests
    {
        [Test]
        public async Task TestParser()
        {
            var dir = new DirectoryInfo(Path.Combine("Psf", "MsixHelper"));
            using IAppxFileReader dirReader = new DirectoryInfoFileReaderAdapter(dir);
            var proxyReader = new MsixHelperProxyReader("ApplicationOne", "MSIXHelper32.exe", dirReader);

            var read = await proxyReader.Inspect().ConfigureAwait(false);

            Assert.That(read, Is.Not.Null);

            Assert.That(read.Executable, Is.EqualTo("%ProgramFiles(x86)%\\MyApp\\MyApplication1.exe"));
            Assert.That(read.Arguments, Is.EqualTo("/test /secondParam /noError"));
            Assert.That(read.WorkingDirectory, Is.EqualTo("%ProgramFiles(x86)%\\MyApp"));
        }
    }
}
