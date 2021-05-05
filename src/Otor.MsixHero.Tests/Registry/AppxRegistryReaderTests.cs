using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Otor.MsixHero.Appx.Packaging.Registry;

namespace Otor.MsixHero.Tests.Registry
{
    public class AppxRegistryReaderTests
    {
        private AppxRegistryReader appxReader;

        [SetUp]
        public void Initialize()
        {
            this.appxReader = new AppxRegistryReader("Registry\\Registry.dat");
        }
        
        [Test]
        public async Task TestReadingFromDatFile()
        {
            var roots = new List<AppxRegistryKey>();
            await foreach (var root in this.appxReader.EnumerateKeys(AppxRegistryRoots.HKLM))
            {
                roots.Add(root);
            }

            Assert.AreEqual(1, roots.Count);
        }
    }
}
