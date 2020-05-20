using System.Collections.Generic;

namespace otor.msixhero.lib.Domain.Winget
{
    /*
     *Id: Publisher.Name
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
  Url: http://sfsfsdfsfs/
  Sha256: e56d336922eaab3be8c1244dbaa713e134a8eba50ddbd4f50fd2fe18d72595cd
     *
     */
    public class YamlInstaller
    {
        public YamlArchitecture? Arch { get; set; }

        public string Url { get; set; }

        public string Sha256 { get; set; }
        
        public string Language { get; set; }

        public string SignatureSha256 { get; set; }
        
        public YamlInstallerType? InstallerType { get; set; } 

        public YamlScope? Scope { get; set; }

        public YamlSwitches Switches { get; set; }
    }

    /*
     * Arch: x64 
    Url: https://cdn1.evernote.com/win6/public/Evernote_6.24.2.8919.exe
    Sha256: 4851DBDB36ECEF1D5F2DE092673B4D70223703F6211FD8A1909B2E3E44AED5F9
    Language: en-US 
    Scope: user
    Switches: 
      Silent: /qn
      SilentWithProgress: /qn
     */
}