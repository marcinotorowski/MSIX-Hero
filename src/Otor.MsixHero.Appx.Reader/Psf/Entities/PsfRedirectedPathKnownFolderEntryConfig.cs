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

namespace Otor.MsixHero.Appx.Reader.Psf.Entities
{
    [DataContract]
    public class PsfRedirectedPathKnownFolderEntryConfig : JsonElement
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "relativePaths")]
        public List<PsfRedirectedPathEntryConfig> RelativePaths { get; set; }

        public override string ToString()
        {
            return $"{this.Id}, {this.RelativePaths?.Count ?? 0} relative paths";
        }
    }
}