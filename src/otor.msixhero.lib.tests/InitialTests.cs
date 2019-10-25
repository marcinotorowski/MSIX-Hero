using NUnit.Framework;
using System;
using System.Linq;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class InitialTests
    {
        [Test]
        public void Test1()
        {
            var man = new AppxPackageManager();
            var allPackages = man.GetPackages().Result.ToList();
            var rvl = allPackages.FirstOrDefault(x => x.Name.Contains("Eval"));
        }
    }
}
