// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfTraceFixupLevels : JsonElement
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "default")]
        public TraceLevel Default { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "filesystem")]
        public TraceLevel Filesystem { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "registry")]
        public TraceLevel Registry { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "processAndThread")]
        public TraceLevel ProcessAndThread { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "dynamicLinkLibrary")]
        public TraceLevel DynamicLinkLibrary { get; set; }

        public override string ToString()
        {
            return
                $"Default: {this.Default}, filesystem: {this.Filesystem}, registry: {this.Registry}, processAndThread: {this.ProcessAndThread}, dynamicLinkLibrary: {this.DynamicLinkLibrary}";
        }
    }
}