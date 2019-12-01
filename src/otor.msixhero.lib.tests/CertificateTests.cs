using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class CertificateTests
    {
        [Test]
        public void TestReader()
        {
            IAppxSigningManager signManager = new AppxSigningManager();
            var certs = signManager.GetCertificatesFromStore(CertificateStoreType.MachineUser).GetAwaiter().GetResult();
        }
    }
}
