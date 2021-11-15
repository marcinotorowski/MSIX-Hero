using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;

namespace Otor.MsixHero.Tests.Appx.Editing
{
    public class ManifestEditingTests
    {
        [Test]
        public async Task AddSimpleMetaDataToEmptyPackage()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4"">
</Package>";

            var manifest = XDocument.Parse(manifestContent);

            var addBuildExecutor = new SetBuildMetaDataExecutor(manifest);
            await addBuildExecutor.Execute(new SetBuildMetaData("componentA", "1.2.3"));

            // ReSharper disable once AssignNullToNotNullAttribute
            var metadata = manifest.Root.XPathSelectElement("//*[local-name()='Metadata']");

            // ReSharper disable once AssignNullToNotNullAttribute
            var item = metadata.XPathSelectElements("//*[local-name()='Item']").ToList();
            Assert.AreEqual(1, item.Count);

            var name = item[0].Attribute("Name")?.Value;
            Assert.AreEqual("componentA", name);
            var version = item[0].Attribute("Version")?.Value;
            Assert.AreEqual("1.2.3", version);
        }

        [Test]
        public async Task AddSimpleMetaDataToNotEmptyPackage()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns:build=""http://schemas.microsoft.com/developer/appx/2015/build""
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4"">
    <build:Metadata>
        <build:Item Name=""test"" Version=""1.0.0"" />
    </build:Metadata>
</Package>";

            var manifest = XDocument.Parse(manifestContent);

            var addBuildExecutor = new SetBuildMetaDataExecutor(manifest);
            await addBuildExecutor.Execute(new SetBuildMetaData("componentA", "1.2.3"));

            // ReSharper disable once AssignNullToNotNullAttribute
            var metadata = manifest.Root.XPathSelectElement("//*[local-name()='Metadata']");

            // ReSharper disable once AssignNullToNotNullAttribute
            var item = metadata.XPathSelectElements("//*[local-name()='Item']").ToList();
            Assert.AreEqual(2, item.Count);

            var name = item[1].Attribute("Name")?.Value;
            Assert.AreEqual("componentA", name);
            var version = item[1].Attribute("Version")?.Value;
            Assert.AreEqual("1.2.3", version);
        }
    }
}
