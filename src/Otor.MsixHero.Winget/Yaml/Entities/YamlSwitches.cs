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
    public class YamlSwitches
    {
        [YamlMember(Order = 1)]
        public string Silent { get; set; }

        [YamlMember(Order = 2)]
        public string SilentWithProgress { get; set; }

        [YamlMember(Order = 3)]
        public string Custom { get; set; }

        [YamlMember(Order = 4)]
        public string Language { get; set; }

        [YamlMember(Order = int.MaxValue)]
        public string Interactive { get; set; }
    }
}