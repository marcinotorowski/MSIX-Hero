// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities
{
    public class PsfRedirectedPathConfig : JsonElement
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageRelative")]
        public List<PsfRedirectedPathEntryConfig> PackageRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "packageDriveRelative")]
        public List<PsfRedirectedPathEntryConfig> PackageDriveRelative { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DataMember(Name = "knownFolders")]
        public List<PsfRedirectedPathKnownFolderEntryConfig> KnownFolders { get; set; }

        public override string ToString()
        {
            return $"{this.PackageRelative?.Count ?? 0} package relative, {this.PackageDriveRelative?.Count ?? 0} package drive relative, {this.KnownFolders?.Count ?? 0} known folder";
        }
    }
}