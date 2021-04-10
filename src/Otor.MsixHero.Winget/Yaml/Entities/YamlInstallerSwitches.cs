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

using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    /// <remarks>
    /// https://github.com/microsoft/winget-cli/blob/c2bf23012848d1bbaa376abc14f5e7c68c64efc1/schemas/JSON/manifests/v1.0.0/manifest.singleton.1.0.0.json#L100
    /// </remarks>
    public class YamlInstallerSwitches
    {
        //  {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "Silent is the value that should be passed to the installer when user chooses a silent or quiet install"
        //  },
        [YamlMember]
        public string Silent { get; set; }

        //  {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "SilentWithProgress is the value that should be passed to the installer when user chooses a non-interactive install"
        //  },
        [YamlMember]
        public string SilentWithProgress { get; set; }

        //  "Interactive": {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "Interactive is the value that should be passed to the installer when user chooses an interactive install"
        //  }
        [YamlMember]
        public string Interactive { get; set; }

        //  "InstallLocation": {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "InstallLocation is the value passed to the installer for custom install location. <INSTALLPATH> token can be included in the switch value so that winget will replace the token with user provided path"
        //  }
        [YamlMember]
        public string InstallLocation { get; set; }

        //  "Log": {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "Log is the value passed to the installer for custom log file path. <LOGPATH> token can be included in the switch value so that winget will replace the token with user provided path"
        //  }
        [YamlMember]
        public string Log { get; set; }

        //  "Upgrade": {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 512,
        //    "description": "Upgrade is the value that should be passed to the installer when user chooses an upgrade"
        //  },
        [YamlMember]
        public string Upgrade { get; set; }
        
        //  "Custom": {
        //    "type": [ "string", "null" ],
        //    "minLength": 1,
        //    "maxLength": 2048,
        //    "description": "Custom switches will be passed directly to the installer by winget"
        //  }
        [YamlMember]
        public string Custom { get; set; }
    }
}