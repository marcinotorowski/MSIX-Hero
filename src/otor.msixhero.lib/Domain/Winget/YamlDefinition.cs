using System.Collections.Generic;

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

namespace otor.msixhero.lib.Domain.Winget
{
    public class YamlDefinition
    {
        public string Id { get; set; }

        public string Publisher { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string License { get; set; }

        public YamlInstallerType? InstallerType { get; set; }

        public string LicenseUrl { get; set; }

        public string AppMoniker { get; set; }

        public string Tags { get; set; }

        public string Description { get; set; }

        public string Commands { get; set; }

        public string Homepage { get; set; }

        public string Author { get; set; }

        public string MinOSVersion { get; set; }

        public IList<YamlInstaller> Installers { get; set; }
    }
}
