using System.IO;
using NUnit.Framework;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.Tests.ViewModels
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
