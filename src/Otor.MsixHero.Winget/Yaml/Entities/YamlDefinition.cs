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
using Version = System.Version;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    /// <see>
    /// https://docs.microsoft.com/en-us/windows/package-manager/package/manifest?tabs=minschema%2Ccompschema
    /// https://github.com/microsoft/winget-cli/blob/master/doc/ManifestSpecv0.1.md
    /// </see>
    public class YamlDefinition
    {
        [YamlMember(Order = 1)]
        public string Id { get; set; }

        [YamlMember(Order = 2)]
        public string Version { get; set; }

        [YamlMember(Order = 3)]
        public string Name { get; set; }

        [YamlMember(Order = 4)]
        public string Publisher { get; set; }

        [YamlMember(Order = 5)]
        public string License { get; set; }

        [YamlMember(Order = 6)]
        public string LicenseUrl { get; set; }

        [YamlMember(Order = 7)]
        public string AppMoniker { get; set; }

        [YamlMember(Order = 8)]
        public string Commands { get; set; }

        [YamlMember(Order = 9)]
        public string Tags { get; set; }

        [YamlMember(Order = 10)]
        public string Description { get; set; }

        [YamlMember(Order = 11)]
        public string Homepage { get; set; }

        [YamlMember(Order = 12)]
        public IList<YamlInstaller> Installers { get; set; }

        [YamlMember(Order = 13, SerializeAs = typeof(string), Alias = "MinOSVersion")]
        public Version MinOperatingSystemVersion { get; set; }

        [YamlMember(Order = 14)]
        [Obsolete("This property should probably be not used...")]
        public string Language { get; set; }

        [YamlMember(Order = 15, SerializeAs = typeof(string))]
        public Version ManifestVersion { get; set; }

        [YamlMember(Order = 16)]
        [Obsolete("This property should probably be not used...")]
        public string Author { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string FileExtensions { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public YamlLocalization Localization { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Protocols { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Channel { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Log { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string InstallLocation { get; set; }

        [YamlMember(Order = int.MaxValue)]
        [Obsolete("This property should probably be not used...")]
        public YamlInstallerType? InstallerType { get; set; }
    }
}
