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
    }
}
