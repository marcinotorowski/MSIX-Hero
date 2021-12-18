using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;

namespace Otor.MsixHero.Tests.Appx.Editing
{
    public class MsixEditTests
    {
        [Test]
        public async Task AddSimpleCapability()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4"">
</Package>";

            var manifest = XDocument.Parse(manifestContent);

            var addCapability = new AddCapability
            {
                Name = "objects3D"
            };

            var executor = new AddCapabilityExecutor(manifest);
            await executor.Execute(addCapability, CancellationToken.None);

            var allCapabilities = manifest.Root.XPathSelectElement("//*[local-name()='Capabilities']");
            var objects3d = allCapabilities.Elements().Single();
            Assert.AreEqual("objects3D", objects3d.Attribute("Name").Value);
            Assert.AreEqual("uap", objects3d.GetPrefixOfNamespace(objects3d.Name.Namespace));
            Assert.IsTrue(manifest.Root.Attribute("IgnorableNamespaces").Value.Split(' ').Contains("uap"));
        }

        [Test]
        public async Task AddSimpleCapabilityWithOtherExisting()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns=""http://schemas.microsoft.com/appx/manifest/foundation/windows10""
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4""
    xmlns:uap=""http://schemas.microsoft.com/appx/manifest/uap/windows10""
    IgnorableNamespaces=""uap uap4"">
    <Capabilities>
        <uap:Capability Name=""test"" />
    </Capabilities>
</Package>";

            var manifest = XDocument.Parse(manifestContent);

            var addCapability = new AddCapability
            {
                Name = "objects3D"
            };

            var executor = new AddCapabilityExecutor(manifest);
            await executor.Execute(addCapability, CancellationToken.None);

            var allCapabilities = manifest.Root.XPathSelectElement("//*[local-name()='Capabilities']");
            var objects3d = allCapabilities.Elements().Last();
            Assert.AreEqual("objects3D", objects3d.Attribute("Name").Value);
            Assert.AreEqual("uap", objects3d.GetPrefixOfNamespace(objects3d.Name.Namespace));
            Assert.AreEqual("uap uap4", manifest.Root.Attribute("IgnorableNamespaces").Value);
        }

        [Test]
        public async Task AddRestrictedCapability()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4"">
</Package>";

            var manifest = XDocument.Parse(manifestContent);
            
            var addCapability = new AddCapability
            {
                Name = "runFullTrust"
            };

            var executor = new AddCapabilityExecutor(manifest);
            await executor.Execute(addCapability, CancellationToken.None);

            var allCapabilities = manifest.Root.XPathSelectElement("//*[local-name()='Capabilities']");
            var runFullTrust = allCapabilities.Elements().Single();
            Assert.AreEqual("runFullTrust", runFullTrust.Attribute("Name").Value);
            Assert.IsTrue(runFullTrust.GetPrefixOfNamespace(runFullTrust.Name.Namespace) == "rescap");
            Assert.IsTrue(manifest.Root.Attribute("IgnorableNamespaces").Value.Split(' ').Contains("rescap"));
        }

        [Test]
        public async Task AddDeviceCapability()
        {
            var manifestContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Package
    xmlns:uap4=""http://schemas.microsoft.com/appx/manifest/uap/windows10/4"">
</Package>";

            var manifest = XDocument.Parse(manifestContent);

            var addCapability = new AddCapability
            {
                Name = "location"
            };

            var executor = new AddCapabilityExecutor(manifest);
            await executor.Execute(addCapability, CancellationToken.None);
            
            var allCapabilities = manifest.Root.XPathSelectElement("//*[local-name()='Capabilities']");
            var location = allCapabilities.Elements().Single();
            Assert.AreEqual("location", location.Attribute("Name").Value);
        }
    }
}
