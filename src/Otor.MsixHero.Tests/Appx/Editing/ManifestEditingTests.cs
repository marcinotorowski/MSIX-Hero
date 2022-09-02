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
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Cli;
using Otor.MsixHero.Cli.Executors.Edit.Psf;
using Otor.MsixHero.Cli.Verbs.Edit.Psf;

namespace Otor.MsixHero.Tests.Appx.Editing
{
    public class ManifestEditingTests
    {
        [Test]
        public async Task InjectPsf()
        {
            var msixHeroPackage = new FileInfo(Path.Combine("Resources", "SamplePackages", "CreatedByMsixHero.msix"));
            var injectPsf = new InjectPsfEditVerb();
            var executor = new InjectPsfVerbExecutor(msixHeroPackage.FullName, injectPsf, new ConsoleImpl(Console.Out, Console.Error));
            await executor.Execute();
        }

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
