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
using System.Xml.Serialization;
using NUnit.Framework;
using Otor.MsixHero.AppInstaller.Entities;

namespace Otor.MsixHero.Tests
{
    [TestFixture]
    public class AppInstallerTests
    {
        [Test]
        public void TestSerialization()
        {
            var stringXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<AppInstaller
    xmlns=""http://schemas.microsoft.com/appx/appinstaller/2017""
    Version=""1.0.0.0""
    Uri=""http://mywebservice.azurewebsites.net/appset.appinstaller"" >

    <MainBundle
        Name=""Contoso.MainApp""
        Publisher=""CN=Contoso""
        Version=""2.23.12.43""
        Uri=""http://mywebservice.azurewebsites.net/mainapp.msixbundle"" />

    <OptionalPackages>
        <Bundle
            Name=""Contoso.OptionalApp1""
            Publisher=""CN=Contoso""
            Version=""2.23.12.43""
            Uri=""http://mywebservice.azurewebsites.net/OptionalApp1.msixbundle"" />

        <Bundle
            Name=""Contoso.OptionalApp2""
            Publisher=""CN=Contoso""
            Version=""2.23.12.43""
            Uri=""http://mywebservice.azurewebsites.net/OptionalApp2.msixbundle"" />

        <Package
            Name=""Fabrikam.OptionalApp3""
            Publisher=""CN=Fabrikam""
            Version=""10.34.54.23""
            ProcessorArchitecture=""x86""
            Uri=""http://mywebservice.azurewebsites.net/OptionalApp3.msix"" />

    </OptionalPackages>

    <Dependencies>
        <Package Name=""Microsoft.VCLibs.140.00"" Publisher=""CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"" Version=""14.0.24605.0"" ProcessorArchitecture=""x86"" Uri=""http://foobarbaz.com/fwkx86.appx"" />
        <Package Name=""Microsoft.VCLibs.140.00"" Publisher=""CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"" Version=""14.0.24605.0"" ProcessorArchitecture=""x64"" Uri=""http://foobarbaz.com/fwkx64.appx"" />
    </Dependencies>

    <UpdateSettings>
        <OnLaunch HoursBetweenUpdateChecks=""12"" />
    </UpdateSettings>

</AppInstaller>
";

            var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig2017));

            using var stream = new StringReader(stringXml);
            var result = (AppInstallerConfig2017) xmlSerializer.Deserialize(stream);

            Assert.That(result.Uri, Is.EqualTo("http://mywebservice.azurewebsites.net/appset.appinstaller"));
            Assert.That(result.Dependencies, Is.Not.Null);
            Assert.That(result.Dependencies.Count, Is.EqualTo(2));
            Assert.That(result.Dependencies[0].Name, Is.EqualTo("Microsoft.VCLibs.140.00"));
            Assert.That(result.Dependencies[0].Version, Is.EqualTo("14.0.24605.0"));
            Assert.That(result.Dependencies[0].Uri, Is.EqualTo("http://foobarbaz.com/fwkx86.appx"));
            Assert.That(((AppInstallerPackageEntry)result.Dependencies[0]).Architecture, Is.EqualTo(AppInstallerPackageArchitecture.x86));
            Assert.That(result.Dependencies[0].Publisher, Is.EqualTo("CN=Microsoft Corporation, O=Microsoft Corporation, L=Redmond, S=Washington, C=US"));

            Assert.That(result.UpdateSettings, Is.Not.Null);
            Assert.That(result.UpdateSettings.OnLaunch, Is.Not.Null);
            Assert.That(result.UpdateSettings.OnLaunch.HoursBetweenUpdateChecks, Is.EqualTo(12));
            Assert.That(result.UpdateSettings.OnLaunch.ShowPrompt, Is.False);
            Assert.That(result.UpdateSettings.OnLaunch.UpdateBlocksActivation, Is.False);
        }
    }
}
