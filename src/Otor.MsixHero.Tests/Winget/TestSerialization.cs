// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.IO;
using NUnit.Framework;
using Otor.MsixHero.Winget.Yaml;
using Otor.MsixHero.Winget.Yaml.Entities;

namespace Otor.MsixHero.Tests.Winget
{
    [TestFixture]
    public class TestSerialization
    {
        [Test]
        [Ignore("To be investigated")]
        public void TestBasic()
        {
            var yaml = @"Id: Google.Chrome
Name: Chrome
Publisher: Google
Version: 78.0.3904.87
Homepage: https://www.google.com/chrome/
InstallerType: MSI
AppMoniker: chrome
License: Google Chrome and Chrome OS Additional Terms of Service
LicenseUrl: https://www.google.com/chrome/terms/

# ======================= Installers  ==================


Installers:
  - Arch: x64
    Url: https://dl.google.com/edgedl/chrome/install/GoogleChromeStandaloneEnterprise64.msi
    Sha256: F17FDA159EC69C0F9E96656642E5F2A5DE547BED784978A354C51D70E88C93DC
    Language: en-US";

            
            var reader = new YamlReader();
            using TextReader textReader = new StringReader(yaml);
            var deserialized = reader.Read(textReader);
            var writer = new YamlWriter();

            using TextWriter textWriter = new StringWriter();
            writer.Write(deserialized, textWriter);
        }

        [Test]
        public void TestEverNote()
        {
            var evernote = @"PackageIdentifier: evernote.evernote
PackageName: Evernote
Moniker: evernote
PackageVersion: 6.24.2.8919
Publisher: Evernote
Author: Evernote
License: Copyright (c) 2020 Evernote Corporation. All rights reserved.
LicenseUrl: https://evernote.com/legal/terms-of-service
MinimumOSVersion: 10.0.0.0
PackageUrl: https://www.evernote.com
ShortDescription: evernote.evernote
Description: Evernote helps you focus on what matters most and have access to your information when you need it. Input typed notes or scan handwritten notes. Add to-do’s, photos, images, web pages, or audio … and it’s all instantly searchable. Organize notes any way you want and share with anyone. And Evernote syncs across your devices so your information is always with you, everywhere you go.
Tags:
- evernote
- notes
- cloud
- online
InstallerType: exe
Installers:
- Architecture: x64
  InstallerUrl: https://cdn1.evernote.com/win6/public/Evernote_6.24.2.8919.exe
  InstallerSha256: 4851DBDB36ECEF1D5F2DE092673B4D70223703F6211FD8A1909B2E3E44AED5F9
  InstallerLocale: en-US
  Scope: user
  InstallerSwitches:
    Silent: /qn
    SilentWithProgress: /qn
PackageLocale: en-US
ManifestType: singleton
ManifestVersion: 1.0.0
";

            var reader = new YamlReader();
            using TextReader textReader = new StringReader(evernote);
            var yaml = reader.Read(textReader);

            Assert.That(yaml.PackageIdentifier, Is.EqualTo("evernote.evernote"));
            Assert.That(yaml.PackageName, Is.EqualTo("Evernote"));
            Assert.That(yaml.Publisher, Is.EqualTo("Evernote"));
            Assert.That(yaml.Moniker, Is.EqualTo("evernote"));
            Assert.That(yaml.License, Is.EqualTo("Copyright (c) 2020 Evernote Corporation. All rights reserved."));
            Assert.That(yaml.LicenseUrl, Is.EqualTo("https://evernote.com/legal/terms-of-service"));
            Assert.That(yaml.PackageVersion, Is.EqualTo("6.24.2.8919"));
            Assert.That(yaml.MinimumOperatingSystemVersion.ToString(), Is.EqualTo("10.0.0.0"));
            Assert.That(yaml.Description, Is.EqualTo("Evernote helps you focus on what matters most and have access to your information when you need it. Input typed notes or scan handwritten notes. Add to-do’s, photos, images, web pages, or audio … and it’s all instantly searchable. Organize notes any way you want and share with anyone. And Evernote syncs across your devices so your information is always with you, everywhere you go."));
            Assert.That(yaml.ShortDescription, Is.EqualTo("evernote.evernote"));
            Assert.That(yaml.PackageUrl, Is.EqualTo("https://www.evernote.com"));
            Assert.That(string.Join(",", yaml.Tags), Is.EqualTo("evernote,notes,cloud,online"));
#pragma warning disable 618
            Assert.That(yaml.Author, Is.EqualTo("Evernote"));
            Assert.That(yaml.InstallerType, Is.EqualTo(YamlInstallerType.Exe));
#pragma warning restore 618
            Assert.That(yaml.Installers, Is.Not.Null);
            Assert.That(yaml.Installers.Count, Is.EqualTo(1));

            var ins = yaml.Installers[0];

            Assert.That(ins.Architecture, Is.EqualTo(YamlArchitecture.X64));
            Assert.That(ins.InstallerUrl, Is.EqualTo("https://cdn1.evernote.com/win6/public/Evernote_6.24.2.8919.exe"));
#pragma warning disable 618
            Assert.That(ins.InstallerLocale, Is.EqualTo("en-US"));
#pragma warning restore 618
            Assert.That(ins.Scope, Is.EqualTo(YamlScope.User));
            Assert.That(ins.InstallerSwitches, Is.Not.Null);

            Assert.That(ins.InstallerSwitches.Silent, Is.EqualTo("/qn"));
            Assert.That(ins.InstallerSwitches.SilentWithProgress, Is.EqualTo("/qn"));
        }
    }
}
