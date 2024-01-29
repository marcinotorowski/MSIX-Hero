// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
            Assert.That(resolvableFolder.Compacted, Is.EqualTo("{{ProgramFilesX86}}\\Test"), "The value must be compacted to ProgramFiles (x86).");

            resolvableFolder.Resolved = "C:\\Program Files\\Test";
            Assert.That(resolvableFolder.Compacted, Is.EqualTo("{{ProgramFiles}}\\Test"));

            resolvableFolder.Compacted = "{{ProgramFilesX86}}\\ABC";
            Assert.That(resolvableFolder.Resolved, Is.EqualTo("C:\\Program Files (x86)\\ABC"));

            resolvableFolder.Compacted = "{{ProgramFiles}}\\ABC";
            Assert.That(resolvableFolder.Resolved, Is.EqualTo("C:\\Program Files\\ABC"));

            resolvableFolder.Compacted = null;
            Assert.That(resolvableFolder.Resolved, Is.EqualTo(resolvableFolder.Compacted), "If compacted path is null, the resolved path must be also null.");

            resolvableFolder.Resolved = null;
            Assert.That(resolvableFolder.Resolved, Is.EqualTo(resolvableFolder.Compacted), "If resolved path is null, the compacted path must be also null.");

            resolvableFolder.Compacted = string.Empty;
            Assert.That(resolvableFolder.Resolved, Is.EqualTo(resolvableFolder.Compacted), "If compacted path is an empty string, the resolved path must be also an empty string.");

            resolvableFolder.Resolved = string.Empty;
            Assert.That(resolvableFolder.Resolved, Is.EqualTo(resolvableFolder.Compacted), "If resolved path is an empty string, the compacted path must be also an empty string.");

            resolvableFolder.Compacted = "abc";
            Assert.That(resolvableFolder.Resolved, Is.EqualTo("abc"), "If compacted path has no variables, the resolved path must be the same value.");

            resolvableFolder.Resolved = "abc";
            Assert.That(resolvableFolder.Compacted, Is.EqualTo("abc"), "If resolved path does not have any resolvable path, then the compacted path must be the same.");
        }

        [Test]
        public void TestImplicitConversion()
        {
            ResolvablePath folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.AdminTools, Environment.SpecialFolderOption.DoNotVerify), "Test");
            Assert.That(folder.Compacted, Is.EqualTo("{{AdminTools}}\\Test"));

            string resolved = folder;
            Assert.That(resolved, Is.EqualTo(folder.Resolved));
        }
        
        [Test]
        public void TestSerialization()
        {
            var folder = new ResolvablePath("{{ProgramFiles}}\\ABC");
            var serialized = JsonConvert.SerializeObject(folder, new ResolvablePathConverter());
            var deserialized = JsonConvert.DeserializeObject<ResolvablePath>(serialized, new ResolvablePathConverter());

            Assert.That(serialized.Contains("\"{{ProgramFiles}}\\\\ABC\"", StringComparison.Ordinal), Is.True, "Serialized object must be a plain type (compacted).");
            Assert.That(folder.Compacted, Is.EqualTo(deserialized.Compacted), "Value before and after serialization must be the same.");
            Assert.That(folder.Resolved, Is.EqualTo(deserialized.Resolved), "Value before and after serialization must be the same.");
        }
    }
}
