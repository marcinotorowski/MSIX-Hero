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
        [YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public YamlArchitecture Arch { get; set; }

        [YamlMember(Order = 2)]
        public string Url { get; set; }

        [YamlMember(Order = 3)]
        public string Sha256 { get; set; }

        [YamlMember(Order = 4)]
        public string SignatureSha256 { get; set; }

        [YamlMember(Order = 5, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public YamlInstallerType InstallerType { get; set; }

        [YamlMember(Order = 6, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public YamlScope Scope { get; set; }

        [YamlMember(Order = 7)]
        public YamlSwitches Switches { get; set; }

        [YamlMember(Order = 9)]
        public string SystemAppId { get; set; }

        [YamlMember(Order = int.MaxValue)]
        [Obsolete("Probably should not be used.")]
        public string Language { get; set; }
    }
}