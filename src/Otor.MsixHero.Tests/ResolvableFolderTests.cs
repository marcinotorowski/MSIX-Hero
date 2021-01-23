// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class ResolvableFolderTests
    {
        [Test]
        public void TestCompacting()
        {
            var resolvableFolder = new ResolvablePath();

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
            ResolvablePath folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools, Environment.SpecialFolderOption.DoNotVerify), "Test");
            Assert.AreEqual("{{AdminTools}}\\Test", folder.Compacted);

            string resolved = folder;
            Assert.AreEqual(folder.Resolved, resolved);
        }
        
        [Test]
        public void TestSerialization()
        {
            var folder = new ResolvablePath("{{ProgramFiles}}\\ABC");
            var serialized = JsonConvert.SerializeObject(folder, new ResolvablePathConverter());
            var deserialized = JsonConvert.DeserializeObject<ResolvablePath>(serialized, new ResolvablePathConverter());

            Assert.IsTrue(serialized.Contains("\"{{ProgramFiles}}\\\\ABC\"", StringComparison.Ordinal), "Serialized object must be a plain type (compacted).");
            Assert.AreEqual(deserialized.Compacted, folder.Compacted, "Value before and after serialization must be the same.");
            Assert.AreEqual(deserialized.Resolved, folder.Resolved, "Value before and after serialization must be the same.");
        }
    }
}
