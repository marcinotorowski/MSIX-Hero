using System;
using System.Linq;
using NUnit.Framework;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Tests.Sdk
{
    [TestFixture]
    public class SignToolSdkTests
    {
        [Test]
        public void TestDeviceGuard()
        {
            var src = @"C:\temp\outfile.json";
            var dev = new DeviceGuardHelper();
            dev.GetSubjectFromDeviceGuardSigning(src, true).GetAwaiter().GetResult();
        }

        [Test]
        public void TestMessage()
        {
            var output = @"The following certificates were considered:
    Issued to: localhost
    Issued by: localhost
    Expires:   Wed May 05 23:39:42 2021
    SHA1 hash: E323874CD3FB890842AB09791CD605F8D3A5340F
    Issued to: ddfe70e5-4d2b-4940-a550-66f4f7d8307c
    Issued by: MS-Organization-Access
    Expires:   Sun May 05 23:57:34 2030
    SHA1 hash: D1AABEE4733FE30E1D6317427E88CA973406025B
    Issued to: Marcin Otorowski
    Issued by: Certum Code Signing CA SHA2
    Expires:   Tue Dec 29 13:55:03 2020
    SHA1 hash: C362045164EFCDA4E40473F0B3B6B1D3E647CA6F
    Issued to: bc64c50169ddad94
    Issued by: Token Signing Public Key
    Expires:   Wed May 13 01:07:14 2020
    SHA1 hash: A33983F854D4C7C6FAC5CC754656DC7CCA5ACEEF
After EKU filter, 1 certs were left.
After expiry filter, 1 certs were left.
After Hash filter, 0 certs were left.
After Private Key filter, 0 certs were left.	
".Split(System.Environment.NewLine).ToList();

            MsixSdkWrapper.TryGetErrorMessageFromSignToolOutput(output, out var error);
            Assert.IsTrue(error.Contains("EKU", StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
