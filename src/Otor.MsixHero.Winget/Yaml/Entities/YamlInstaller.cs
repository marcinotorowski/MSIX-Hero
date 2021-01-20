using System;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    /*

    Sample:
    
Id: Publisher.Name
Publisher: Publisher
Name: Name
Version: Version
License: License
InstallerType: MSIX|EXE|MSI
LicenseUrl: LicenseURL
AppMoniker: Alternate name
Tags: Keywords
Description: Description
Homepage: Homepage
Installers:
- Arch: x86 x64|arm|arm64|x86
  Url: http://msixhero.net/download.msix
  Sha256: e56d336922eaab3be8c1244dbaa713e134a8eba50ddbd4f50fd2fe18d72595cd
     */
    
    public class YamlInstaller
    {
        [YamlMember(Order = 1)]
        public YamlArchitecture? Arch { get; set; }

        [YamlMember(Order = 2)]
        public string Url { get; set; }

        [YamlMember(Order = 3)]
        public string Sha256 { get; set; }

        [YamlMember(Order = 4)]
        public string SignatureSha256 { get; set; }

        [YamlMember(Order = 5)]
        public YamlInstallerType? InstallerType { get; set; }

        [YamlMember(Order = 6)]
        public YamlScope? Scope { get; set; }

        [YamlMember(Order = 7)]
        public YamlSwitches Switches { get; set; }

        [YamlMember(Order = 9)]
        public string SystemAppId { get; set; }

        [YamlMember(Order = int.MaxValue)]
        [Obsolete("Probably should not be used.")]
        public string Language { get; set; }
    }
}