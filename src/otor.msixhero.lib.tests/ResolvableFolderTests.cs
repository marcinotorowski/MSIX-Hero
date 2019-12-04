using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Configuration.ResolvableFolder;

namespace otor.msixhero.lib.tests
{
    [TestFixture]
    public class ResolvableFolderTests
    {
        [Test]
        public void TestCompacting()
        {
            var resolvableFolder = new ResolvableFolder();

            resolvableFolder.Resolved = "C:\\Program Files (x86)\\Test";
            Assert.AreEqual("{{ProgramFilesX86}}\\Test", resolvableFolder.Compacted, "The value must be compacted to ProgramFiles (x86).");

            resolvableFolder.Resolved = "C:\\Program Files\\Test";
            Assert.AreEqual("{{ProgramFiles}}\\Test", resolvableFolder.Compacted);

            resolvableFolder.Compacted = "{{ProgramFilesX86}}\\ABC";
            Assert.AreEqual("C:\\Program Files (x86)\\ABC", resolvableFolder.Resolved);

            resolvableFolder.Compacted = "{{ProgramFiles}}\\ABC";
            Assert.AreEqual("C:\\Program Files\\ABC", resolvableFolder.Resolved);

            resolvableFolder.Compacted = null;
            Assert.AreEqual(resolvableFolder.Compacted, resolvableFolder.Resolved, "If compacted path is null, the resolved path must be also null.");

            resolvableFolder.Resolved = null;
            Assert.AreEqual(resolvableFolder.Compacted, resolvableFolder.Resolved, "If resolved path is null, the compacted path must be also null.");

            resolvableFolder.Compacted = string.Empty;
            Assert.AreEqual(resolvableFolder.Compacted, resolvableFolder.Resolved, "If compacted path is an empty string, the resolved path must be also an empty string.");

            resolvableFolder.Resolved = string.Empty;
            Assert.AreEqual(resolvableFolder.Compacted, resolvableFolder.Resolved, "If resolved path is an empty string, the compacted path must be also an empty string.");

            resolvableFolder.Compacted = "abc";
            Assert.AreEqual("abc", resolvableFolder.Resolved, "If compacted path has no variables, the resolved path must be the same value.");

            resolvableFolder.Resolved = "abc";
            Assert.AreEqual("abc", resolvableFolder.Compacted, "If resolved path does not have any resolvable path, then the compacted path must be the same.");
        }

        [Test]
        public void TestImplicitConversion()
        {
            ResolvableFolder folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools, Environment.SpecialFolderOption.DoNotVerify), "Test");
            Assert.AreEqual("{{AdminTools}}\\Test", folder.Compacted);

            string resolved = folder;
            Assert.AreEqual(folder.Resolved, resolved);
        }
        
        [Test]
        public void TestSerialization()
        {
            var folder = new ResolvableFolder("{{ProgramFiles}}\\ABC");
            var serialized = JsonConvert.SerializeObject(folder, new ResolvableFolderConverter());
            var deserialized = JsonConvert.DeserializeObject<ResolvableFolder>(serialized, new ResolvableFolderConverter());

            Assert.IsTrue(serialized.Contains("\"{{ProgramFiles}}\\\\ABC\"", StringComparison.Ordinal), "Serialized object must be a plain type (compacted).");
            Assert.AreEqual(deserialized.Compacted, folder.Compacted, "Value before and after serialization must be the same.");
            Assert.AreEqual(deserialized.Resolved, folder.Resolved, "Value before and after serialization must be the same.");
        }
    }
}
