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
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    public abstract class BaseYamlInstaller
    {
        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([a-zA-Z]{2}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$",
        //   "maxLength": 20,
        //   "description": "The package meta-data locale"
        // }
        [YamlMember]
        public string InstallerLocale { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "title": "Platform",
        //     "type": "string",
        //     "enum": [
        //       "Windows.Desktop",
        //       "Windows.Universal"
        //     ]
        //   },
        //   "maxItems": 2,
        //   "uniqueItems": true,
        //   "description": "The installer supported operating system"
        // }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public List<YamlPlatform> Platform { get; set; }

        //  {
        //    "type": [ "string", "null" ],
        //    "pattern": "^(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){0,3}$",
        //    "description": "The installer minimum operating system version"
        //  }
        [YamlMember(SerializeAs = typeof(string), Alias = "MinimumOSVersion")]
        public Version MinimumOperatingSystemVersion { get; set; }

        //  {
        //    "type": "string",
        //    "enum": [
        //      "x86",
        //      "x64",
        //      "arm",
        //      "arm64",
        //      "neutral"
        //    ],
        //    "description": "The installer target architecture"
        //  },
        [YamlMember(Order = 1, DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public YamlArchitecture Architecture { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "enum": [
        //     "msix",
        //     "msi",
        //     "appx",
        //     "exe",
        //     "zip",
        //     "inno",
        //     "nullsoft",
        //     "wix",
        //     "burn",
        //     "pwa"
        //   ],
        //   "description": "Enumeration of supported installer types"
        // }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public YamlInstallerType InstallerType { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "enum": [
        //     "user",
        //     "machine"
        //   ],
        //   "description": "Scope indicates if the installer is per user or per machine"
        // }
        [YamlMember]
        public YamlScope Scope { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "title": "InstallModes",
        //     "type": "string",
        //     "enum": [
        //       "interactive",
        //       "silent",
        //       "silentWithProgress"
        //     ]
        //   },
        //   "maxItems": 3,
        //   "uniqueItems": true,
        //   "description": "List of supported installer modes"
        // }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public List<YamlInstallMode> InstallModes { get; set; }

        // {
        //   "type": "object"
        // }
        [YamlMember]
        public YamlInstallerSwitches InstallerSwitches { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "integer",
        //     "not": {
        //       "enum": [ 0 ]
        //     }
        //   },
        //   "maxItems": 16,
        //   "uniqueItems": true,
        //   "description": "List of additional non-zero installer success exit codes other than known default values by winget"
        // }
        [YamlMember]
        public List<int> InstallerSuccessCodes { get; set; }

        // {
        //     "type": [ "string", "null" ],
        //     "enum": [
        //     "install",
        //     "uninstallPrevious"
        //         ],
        //     "description": "The upgrade method"
        // }
        [YamlMember]
        public YamlUpgradeBehavior UpgradeBehavior { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "string",
        //     "minLength": 1,
        //     "maxLength": 40
        //   },
        //   "maxItems": 16,
        //   "uniqueItems": true,
        //   "description": "List of commands or aliases to run the package"
        // }
        [YamlMember]
        public List<string> Commands { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "string",
        //     "pattern": "^[a-z][-a-z0-9\\.\\+]*$",
        //     "maxLength": 2048
        //   },
        //   "maxItems": 16,
        //   "uniqueItems": true,
        //   "description": "List of protocols the package provides a handler for"
        // }
        [YamlMember]
        public List<string> Protocols { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "string",
        //     "pattern": "^[^\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]+$",
        //     "maxLength": 40
        //   },
        //   "maxItems": 256,
        //   "uniqueItems": true,
        //   "description": "List of file extensions the package could support"
        // }
        [YamlMember]
        public List<string> FileExtensions { get; set; }

        // {
        //   "type": [ "object", "null" ],
        // }
        [YamlMember]
        public YamlDependencies Dependencies { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$",
        //   "maxLength": 255,
        //   "description": "PackageFamilyName for appx or msix installer. Could be used for correlation of packages across sources"
        // }
        [YamlMember]
        public string PackageFamilyName { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "minLength": 1,
        //   "maxLength": 255,
        //   "description": "ProductCode could be used for correlation of packages across sources"
        // }
        [YamlMember(Order = 16)]
        public string ProductCode { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "string",
        //     "minLength": 1,
        //     "maxLength": 40
        //   },
        //   "maxItems": 1000,
        //   "uniqueItems": true,
        //   "description": "List of appx or msix installer capabilities"
        // }
        [YamlMember(Alias = "Capabilities")]
        public List<string> Capabilities { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": "string",
        //     "minLength": 1,
        //     "maxLength": 40
        //   },
        //   "maxItems": 1000,
        //   "uniqueItems": true,
        //   "description": "List of appx or msix installer restricted capabilities"
        // }
        [YamlMember(Alias = "RestrictedCapabilities")]
        public List<string> RestrictedCapabilities { get; set; }
    }
}