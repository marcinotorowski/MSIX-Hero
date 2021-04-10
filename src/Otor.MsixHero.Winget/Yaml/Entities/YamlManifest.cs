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
using System.Collections.Generic;
using YamlDotNet.Serialization;
using Version = System.Version;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    /// <see>
    /// https://github.com/microsoft/winget-cli/blob/master/schemas/JSON/manifests/v1.0.0/manifest.singleton.1.0.0.json
    /// </see>
    // "required": [
    //   "PackageIdentifier",
    //   "PackageVersion",
    //   "PackageLocale",
    //   "Publisher",
    //   "PackageName",
    //   "License",
    //   "ShortDescription",
    //   "Installers",
    //   "ManifestType",
    //   "ManifestVersion"
    // ]
    public class YamlManifest : BaseYamlInstaller
    {
        // {
        //   "type": "string",
        //   "pattern": "^[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}(\\.[^\\.\\s\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]{1,32}){1,3}$",
        //   "maxLength": 128,
        //   "description": "The package unique identifier"
        // }
        [YamlMember]
        public string PackageIdentifier { get; set; }

        // {
        //   "type": "string",
        //   "pattern": "^[^\\\\/:\\*\\?\"<>\\|\\x01-\\x1f]+$",
        //   "maxLength": 128,
        //   "description": "The package version"
        // }
        [YamlMember]
        public string PackageVersion { get; set; }

        // {
        //   "type": "string",
        //   "pattern": "^([a-zA-Z]{2}|[iI]-[a-zA-Z]+|[xX]-[a-zA-Z]{1,8})(-[a-zA-Z]{1,8})*$",
        //   "maxLength": 20,
        //   "description": "The package meta-data locale"
        // }
        [YamlMember]
        public string PackageLocale { get; set; }

        // {
        //   "type": "string",
        //   "minLength": 2,
        //   "maxLength": 256,
        //   "description": "The publisher name"
        // }
        [YamlMember]
        public string Publisher { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "The publisher home page"
        // }
        [YamlMember]
        public string PublisherUrl { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "The publisher support page"
        // }
        [YamlMember]
        public string PublisherSupportUrl { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "The publisher privacy page or the package privacy page"
        // }
        [YamlMember]
        public string PrivacyUrl { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "minLength": 2,
        //   "maxLength": 256,
        //   "description": "The package author"
        // }
        [YamlMember]
        public string Author { get; set; }

        // {
        //   "type": "string",
        //   "minLength": 2,
        //   "maxLength": 256,
        //   "description": "The package name"
        // }
        [YamlMember]
        public string PackageName { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "The package home page"
        // }
        [YamlMember]
        public string PackageUrl { get; set; }

        // {
        //   "type": "string",
        //   "minLength": 3,
        //   "maxLength": 512,
        //   "description": "The package license"
        // }
        [YamlMember]
        public string License { get; set; }
        
        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "Optional Url type"
        // }
        [YamlMember]
        public string LicenseUrl { get; set; }

        // {
        //   "type": "string",
        //   "minLength": 3,
        //   "maxLength": 512,
        //   "description": "The package copyright"
        // }
        [YamlMember]
        public string Copyright { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "pattern": "^([Hh][Tt][Tt][Pp][Ss]?)://.+$",
        //   "maxLength": 2000,
        //   "description": "The package copyright page"
        // }
        [YamlMember]
        public string CopyrightUrl { get; set; }

        // {
        //   "type": "string",
        //   "minLength": 3,
        //   "maxLength": 256,
        //   "description": "The short package description"
        // }
        [YamlMember]
        public string ShortDescription { get; set; }

        // {
        //   "type": [ "string", "null" ],
        //   "minLength": 3,
        //   "maxLength": 10000,
        //   "description": "The full package description"
        // }
        [YamlMember]
        public string Description { get; set; }
        
        // {
        //   "type": [ "string", "null" ],
        //   "minLength": 1,
        //   "maxLength": 40,
        //   "description": "Package moniker or tag"
        // }
        [YamlMember]
        public string Moniker { get; set; }

        // {
        //   "type": [ "array", "null" ],
        //   "items": {
        //     "type": [ "string", "null" ],
        //     "minLength": 1,
        //     "maxLength": 40,
        //     "description": "Package moniker or tag"
        //   },
        //   "maxItems": 16,
        //   "uniqueItems": true,
        //   "description": "List of additional package search terms"
        // }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
        public List<string> Tags { get; set; }

        //
        // {
        //   "type": [ "string", "null" ],
        //   "minLength": 1,
        //   "maxLength": 16,
        //   "description": "The distribution channel"
        // }
        [YamlMember]
        public string Channel { get; set; }
        
        // {
        //   "type": "array",
        //   "items": {
        //     "$ref": "#/definitions/Installer"
        //   },
        //   "minItems": 1,
        //   "maxItems": 1
        //  }
        [YamlMember]
        public List<YamlInstaller> Installers { get; set; }
        
        // {
        //   "type": "string",
        //   "default": "singleton",
        //   "const": "singleton",
        //   "description": "The manifest type"
        // }
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.Preserve)] 
        public string ManifestType { get; set; } = "singleton";

        // {
        //   "type": "string",
        //   "default": "1.0.0",
        //   "pattern": "^(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])(\\.(0|[1-9][0-9]{0,3}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])){2}$",
        //   "description": "The manifest syntax version"
        // }
        [YamlMember(SerializeAs = typeof(string))]
        public Version ManifestVersion { get; set; } = new Version(1, 0 ,0);
    }
}
