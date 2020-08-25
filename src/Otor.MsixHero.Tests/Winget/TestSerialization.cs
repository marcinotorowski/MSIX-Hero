using NUnit.Framework;
using Otor.MsixHero.Winget.Yaml;
using Otor.MsixHero.Winget.Yaml.Entities;

namespace Otor.MsixHero.Tests.Winget
{
    [TestFixture]
    public class TestSerialization
    {
        [Test]
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
            var deserialized = reader.Read(yaml);

            var writer = new YamlWriter();
            var serialized = writer.Write(deserialized);
        }

        [Test]
        public void TestEverNote()
        {
            var evernote = @"Id: evernote.evernote
Name: Evernote
AppMoniker: evernote
Version: 6.24.2.8919
Publisher: Evernote
Author: Evernote
License: Copyright (c) 2020 Evernote Corporation. All rights reserved.
LicenseUrl: https://evernote.com/legal/terms-of-service
MinOSVersion: 10.0.0.0
Homepage: https://www.evernote.com
Description: Evernote helps you focus on what matters most and have access to your information when you need it. Input typed notes or scan handwritten notes. Add to-do’s, photos, images, web pages, or audio ... and it's all instantly searchable. Organize notes any way you want and share with anyone. And Evernote syncs across your devices so your information is always with you, everywhere you go.
Tags: ""evernote,notes,cloud,online""
InstallerType: exe
Installers: 
  - Arch: x64 
    Url: https://cdn1.evernote.com/win6/public/Evernote_6.24.2.8919.exe
    Sha256: 4851DBDB36ECEF1D5F2DE092673B4D70223703F6211FD8A1909B2E3E44AED5F9
    Language: en-US 
    Scope: user
    Switches: 
      Silent: /qn
      SilentWithProgress: /qn";

            var reader = new YamlReader();
            var yaml = reader.Read(evernote);

            Assert.AreEqual("evernote.evernote", yaml.Id);
            Assert.AreEqual("Evernote", yaml.Name);
            Assert.AreEqual("Evernote", yaml.Publisher);
            Assert.AreEqual("Evernote", yaml.Author);
            Assert.AreEqual("evernote", yaml.AppMoniker);
            Assert.AreEqual("Copyright (c) 2020 Evernote Corporation. All rights reserved.", yaml.License);
            Assert.AreEqual("https://evernote.com/legal/terms-of-service", yaml.LicenseUrl);
            Assert.AreEqual("6.24.2.8919", yaml.Version);
            Assert.AreEqual("10.0.0.0", yaml.MinOperatingSystemVersion.ToString());
            Assert.AreEqual("Evernote helps you focus on what matters most and have access to your information when you need it. Input typed notes or scan handwritten notes. Add to-do’s, photos, images, web pages, or audio ... and it's all instantly searchable. Organize notes any way you want and share with anyone. And Evernote syncs across your devices so your information is always with you, everywhere you go.", yaml.Description);
            Assert.AreEqual("https://www.evernote.com", yaml.Homepage);
            Assert.AreEqual("evernote,notes,cloud,online", yaml.Tags);
            Assert.AreEqual(YamlInstallerType.exe, yaml.InstallerType);
            Assert.NotNull(yaml.Installers);
            Assert.AreEqual(1, yaml.Installers.Count);

            var ins = yaml.Installers[0];

            Assert.AreEqual(YamlArchitecture.x64, ins.Arch);
            Assert.AreEqual("https://cdn1.evernote.com/win6/public/Evernote_6.24.2.8919.exe", ins.Url);
#pragma warning disable 618
            Assert.AreEqual("en-US", ins.Language);
#pragma warning restore 618
            Assert.AreEqual(YamlScope.user, ins.Scope);
            Assert.NotNull(ins.Switches);

            Assert.AreEqual("/qn", ins.Switches.Silent);
            Assert.AreEqual("/qn", ins.Switches.SilentWithProgress);
        }
    }
}
