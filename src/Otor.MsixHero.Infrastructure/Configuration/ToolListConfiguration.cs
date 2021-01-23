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

using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract]
    public class ToolListConfiguration : BaseJsonSetting
    {
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [DataMember(Name = "path")]
        [JsonProperty(PropertyName = "path", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ResolvableFolder.ResolvablePath Path { get; set; }

        [DataMember(Name = "icon")]
        [JsonProperty(PropertyName = "icon", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public ResolvableFolder.ResolvablePath Icon { get; set; }

        [DataMember(Name = "asAdmin")]
        [JsonProperty(PropertyName = "asAdmin")]
        public bool AsAdmin { get; set; }

        [DataMember(Name = "arguments")]
        [JsonProperty(PropertyName = "arguments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Arguments { get; set; }
    }
}