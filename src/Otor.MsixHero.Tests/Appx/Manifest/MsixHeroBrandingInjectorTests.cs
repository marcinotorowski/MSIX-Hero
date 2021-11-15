using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Facades;

namespace Otor.MsixHero.Tests.Appx.Manifest
{
    public class MsixHeroBrandingInjectorTests
    {
        [Test]
        public async Task TestSimpleInjection()
        {
            var manifest = this.PrepareMockManifest();

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest).ConfigureAwait(false);

            Assert.NotNull(GetBuildVersion(manifest, "MsixHero"));
            Assert.NotNull(GetBuildVersion(manifest, "OperatingSystem"));
            Assert.NotNull(GetBuildVersion(manifest, "SignTool.exe"));
            Assert.NotNull(GetBuildVersion(manifest, "MakePri.exe"));
            Assert.NotNull(GetBuildVersion(manifest, "MakeAppx.exe"));
        }

        [Test]
        public async Task TestOverridingDifferentCasing()
        {
            var existingValues = new Dictionary<string, string>
            {
                { "msixHero", "91.0" },
                { "operatingSystem", "92.0" },
                { "MAKEPri.exe", "93.0" },
                { "SIGNTool.exe", "94.0" },
                { "MakeAPPX.exe", "95.0" },
            };

            var manifest = this.PrepareMockManifest(existingValues);

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferIncoming);

            Assert.AreNotEqual("91.0", GetBuildVersion(manifest, "MsixHero"), "By default this value must be overridden.");
            Assert.AreNotEqual("92.0", GetBuildVersion(manifest, "OperatingSystem"), "By default this value must be overridden.");
            Assert.AreNotEqual("93.0", GetBuildVersion(manifest, "MakePri.exe"), "By default this value must be overridden.");
            Assert.AreNotEqual("94.0", GetBuildVersion(manifest, "SignTool.exe"), "By default this value must be overridden.");
            Assert.AreNotEqual("95.0", GetBuildVersion(manifest, "MakeAppx.exe"), "By default this value must be overridden.");
        }

        [Test]
        public async Task TestOverridingMissingExtensions()
        {
            var existingValues = new Dictionary<string, string>
            {
                { "MsixHero", "91.0" },
                { "OperatingSystem", "92.0" },
                { "MakePri", "93.0" },
                { "SignTool.exe", "94.0" },
                { "MakeAppx", "95.0" },
            };

            var manifest = this.PrepareMockManifest(existingValues);

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferIncoming);

            Assert.AreNotEqual("91.0", GetBuildVersion(manifest, "MsixHero"), "By default this value must be overridden.");
            Assert.AreNotEqual("92.0", GetBuildVersion(manifest, "OperatingSystem"), "By default this value must be overridden.");
            Assert.AreNotEqual("93.0", GetBuildVersion(manifest, "MakePri.exe"), "By default this value must be overridden.");
            Assert.AreNotEqual("94.0", GetBuildVersion(manifest, "SignTool.exe"), "By default this value must be overridden.");
            Assert.AreNotEqual("95.0", GetBuildVersion(manifest, "MakeAppx.exe"), "By default this value must be overridden.");
        }

        [Test]
        public async Task TestOverridingDefault()
        {
            var existingValues = new Dictionary<string, string>
            {
                { "MsixHero", "91.0" },
                { "OperatingSystem", "92.0" },
                { "MakePri.exe", "93.0" },
                { "SignTool.exe", "94.0" },
                { "MakeAppx.exe", "95.0" },
            };

            var manifest = this.PrepareMockManifest(existingValues);

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest);

            Assert.AreNotEqual("91.0", GetBuildVersion(manifest, "MsixHero"), "By default this value must be overridden.");
            Assert.AreEqual("92.0", GetBuildVersion(manifest, "OperatingSystem"), "By default this value must not be overridden.");
            Assert.AreEqual("93.0", GetBuildVersion(manifest, "MakePri.exe"), "By default this value must not be overridden.");
            Assert.AreNotEqual("94.0", GetBuildVersion(manifest, "SignTool.exe"), "By default this value must be overridden.");
            Assert.AreNotEqual("95.0", GetBuildVersion(manifest, "MakeAppx.exe"), "By default this value must be overridden.");
        }

        [Test]
        public async Task TestOverridingPreferExisting()
        {
            var existingValues = new Dictionary<string, string>
            {
                { "MsixHero", "91.0" },
                { "OperatingSystem", "92.0" },
                { "MakePri.exe", "93.0" },
                { "SignTool.exe", "94.0" },
                { "MakeAppx.exe", "95.0" },
            };

            var manifest = this.PrepareMockManifest(existingValues);

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferExisting);

            Assert.AreNotEqual("91.0", GetBuildVersion(manifest, "MsixHero"), "This value must be always overridden.");
            Assert.AreEqual("92.0", GetBuildVersion(manifest, "OperatingSystem"), "This value must not be overridden.");
            Assert.AreEqual("93.0", GetBuildVersion(manifest, "MakePri.exe"), "This value must not be overridden.");
            Assert.AreEqual("94.0", GetBuildVersion(manifest, "SignTool.exe"), "This value must not be overridden.");
            Assert.AreEqual("95.0", GetBuildVersion(manifest, "MakeAppx.exe"), "This value must not be overridden.");

            var existingIncompleteValues = new Dictionary<string, string>
            {
                { "SignTool.exe", "94.0" },
                { "MakeAppx.exe", "95.0" },
            };

            manifest = this.PrepareMockManifest(existingIncompleteValues);

            await  injector.Inject(manifest, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferExisting);

            Assert.NotNull(GetBuildVersion(manifest, "MsixHero"));
            Assert.NotNull(GetBuildVersion(manifest, "OperatingSystem"));
            Assert.NotNull(GetBuildVersion(manifest, "MakePri.exe"));
            Assert.AreEqual("94.0", GetBuildVersion(manifest, "SignTool.exe"), "This value must not be overridden.");
            Assert.AreEqual("95.0", GetBuildVersion(manifest, "MakeAppx.exe"), "This value must not be overridden.");
        }

        [Test]
        public async Task TestOverridingPreferIncoming()
        {
            var existingValues = new Dictionary<string, string>
            {
                { "MsixHero", "91.0" },
                { "OperatingSystem", "92.0" },
                { "MakePri.exe", "93.0" },
                { "SignTool.exe", "94.0" },
                { "MakeAppx.exe", "95.0" },
            };

            var manifest = this.PrepareMockManifest(existingValues);

            var injector = new MsixHeroBrandingInjector();
            await injector.Inject(manifest, MsixHeroBrandingInjector.BrandingInjectorOverrideOption.PreferIncoming);

            Assert.AreNotEqual("91.0", GetBuildVersion(manifest, "MsixHero"), "This value must be overridden.");
            Assert.AreNotEqual("92.0", GetBuildVersion(manifest, "OperatingSystem"), "This value must be overridden.");
            Assert.AreNotEqual("93.0", GetBuildVersion(manifest, "MakePri"), "This value must be overridden.");
            Assert.AreNotEqual("94.0", GetBuildVersion(manifest, "SignTool"), "This value must be overridden.");
            Assert.AreNotEqual("95.0", GetBuildVersion(manifest, "MakeAppx"), "This value must be overridden.");
        }

        private XDocument PrepareMockManifest()
        {
            XNamespace windows10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            var manifest = new XDocument();
            manifest.Add(new XElement(windows10Namespace + "Package"));
            return manifest;
        }

        private XDocument PrepareMockManifest(IEnumerable<KeyValuePair<string, string>> nameAndVersions)
        {
            XNamespace windows10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            var manifest = new XDocument();
            var package = new XElement(windows10Namespace + "Package");
            manifest.Add(package);

            var buildNamespace = XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");
            var metaData = new XElement(buildNamespace + "Metadata");
            package.Add(metaData);

            foreach (var item in nameAndVersions)
            {
                var node = new XElement(buildNamespace + "Item");
                node.Add(new XAttribute("Name", item.Key));
                node.Add(new XAttribute("Version", item.Value));
                metaData.Add(node);
            }

            return manifest;
        }

        private string GetBuildVersion(XDocument document, string name)
        {
            XNamespace windows10Namespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
            XNamespace appxNamespace = "http://schemas.microsoft.com/appx/2010/manifest";
            XNamespace uap4Namespace = XNamespace.Get("http://schemas.microsoft.com/appx/manifest/uap/windows10/4");
            XNamespace buildNamespace = XNamespace.Get("http://schemas.microsoft.com/developer/appx/2015/build");
            
            var package = document.Element(windows10Namespace + "Package") ?? document.Element(appxNamespace + "Package");
            if (package == null)
            {
                throw new ArgumentException("Missing <Package /> element.", nameof(document));
            }

            var metaData = package.Element(buildNamespace + "Metadata");

            if (metaData == null)
            {
                throw new ArgumentException("Missing <Metadata /> element.", nameof(document));
            }

            var node = metaData.Elements(buildNamespace + "Item").FirstOrDefault(item => string.Equals(item.Attribute("Name")?.Value, name, StringComparison.OrdinalIgnoreCase));
            if (node == null)
            {
                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                    node = metaData.Elements(buildNamespace + "Item").FirstOrDefault(item => string.Equals(item.Attribute("Name")?.Value, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                    if (node == null)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            var attribute = node.Attribute("Version");
            if (attribute == null)
            {
                return null;
            }

            return attribute.Value;
        }
    }
}
