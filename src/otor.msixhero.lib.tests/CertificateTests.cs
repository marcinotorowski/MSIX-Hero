using System.IO;
using System.Threading;
using NUnit.Framework;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class CertificateTests
    {
        [Test]
        public void TestReader()
        {
            ISigningManager signManager = new SigningManager();
            var certs = signManager.GetCertificatesFromStore(CertificateStoreType.MachineUser).GetAwaiter().GetResult();
        }

        [Test]
        public void TestCertValidator()
        {
            var fileSignedButUntrusted = new FileInfo(Path.Combine("Resources", "ConEmuPack-O2004-M1220.603-P380-F_19.1.8.0_x64__xwfzvwzp69w2e.msix"));
            var fileUnsigned = new FileInfo(Path.Combine("Resources", "nsis.exe"));
            var fileSignedAndTrusted = new FileInfo(Path.Combine("Resources", "inno.exe"));
            var fileNotSignable = new FileInfo(Path.Combine("Resources", "wintrust.dll.ini"));

            var manager = new SigningManager();

            var check1 = manager.IsTrusted(fileSignedButUntrusted.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check2 = manager.IsTrusted(fileSignedAndTrusted.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check3 = manager.IsTrusted(fileUnsigned.FullName, CancellationToken.None).GetAwaiter().GetResult();
            var check4 = manager.IsTrusted(fileNotSignable.FullName, CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(check1.IsTrusted);
            Assert.IsTrue(check2.IsTrusted);
            Assert.IsFalse(check3.IsTrusted);
            Assert.IsFalse(check4.IsTrusted);
        }
    }
}
