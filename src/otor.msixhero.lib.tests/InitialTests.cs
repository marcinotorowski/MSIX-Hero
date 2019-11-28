using NUnit.Framework;
using System;
using System.Linq;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.Signing;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class InitialTests
    {
        [Test]
        public void Test1()
        {
            var man = new CurrentUserAppxPackageManager(new AppxSigningManager());
            var allPackages = man.Get().Result.ToList();
            var rvl = allPackages.FirstOrDefault(x => x.Name.Contains("Eval"));
        }
    }
}
