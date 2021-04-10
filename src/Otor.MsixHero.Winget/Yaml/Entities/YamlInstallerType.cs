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

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    /// <remarks>
    /// https://github.com/microsoft/winget-cli/blob/c2bf23012848d1bbaa376abc14f5e7c68c64efc1/schemas/JSON/manifests/v1.0.0/manifest.singleton.1.0.0.json#L63
    /// </remarks>
    public enum YamlInstallerType
    {
        [System.Runtime.Serialization.EnumMember(Value = "")]
        None = 0,

        [System.Runtime.Serialization.EnumMember(Value = "exe")]
        Exe,

        [System.Runtime.Serialization.EnumMember(Value = "msi")]
        Msi,

        [System.Runtime.Serialization.EnumMember(Value = "msix")]
        Msix,

        // ReSharper disable once StringLiteralTypo
        [System.Runtime.Serialization.EnumMember(Value = "inno")]
        // ReSharper disable once IdentifierTypo
        InnoSetup,

        [System.Runtime.Serialization.EnumMember(Value = "wix")]
        Wix,
        // ReSharper disable once StringLiteralTypo

        [System.Runtime.Serialization.EnumMember(Value = "nullsoft")]
        // ReSharper disable once IdentifierTypo
        Nullsoft,

        [System.Runtime.Serialization.EnumMember(Value = "appx")]
        Appx,

        [System.Runtime.Serialization.EnumMember(Value = "zip")]
        Zip,

        [System.Runtime.Serialization.EnumMember(Value = "burn")]
        Burn,

        [System.Runtime.Serialization.EnumMember(Value = "pwa")]
        ProgressiveWebApp
    }
}