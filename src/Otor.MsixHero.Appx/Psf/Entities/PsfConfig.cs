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

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfConfig : JsonElement
    {
        [DataMember(Name = "applications")]
        public List<PsfApplication> Applications { get; set; }

        [DataMember(Name = "processes")]
        public List<PsfProcess> Processes { get; set; }

        public override string ToString()
        {
            return $"{this.Applications?.Count ?? 0} applications, {this.Processes?.Count ?? 0} processes";
        }
    }
}
