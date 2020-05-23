using System.IO;
using NUnit.Framework;
using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.lib.tests.ViewModels
{
    [TestFixture]
    public class PsfExpertTests
    {
        [Test]
        [Ignore("integration")]
        public void TestBuild()
        {
            var serializer = new PsfConfigSerializer();
            var json = File.ReadAllText(@"E:\temp\msix-psf\fixed-rayeval\config.json");
            var config = serializer.Deserialize(json);

        }
    }
}
