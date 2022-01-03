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

namespace Otor.MsixHero.Appx.Psf.Entities
{
    [DataContract]
    public class PsfApplication : JsonElement
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "executable")]
        public string Executable { get; set; }

        [DataMember(Name = "workingDirectory")]
        public string WorkingDirectory { get; set; }

        [DataMember(Name = "arguments")]
        public string Arguments { get; set; }
        
        [DataMember(Name = "startScript", EmitDefaultValue = false)]
        public PsfScript StartScript { get; set; }
        
        [DataMember(Name = "endScript", EmitDefaultValue = false)]
        public PsfScript EndScript { get; set; }

        /// <summary>
        /// Specifies whether to exit the application if the starting script fails.
        /// </summary>
        [DataMember(Name = "stopOnScriptError")]
        public bool StopOnScriptError { get; set; }

        public override string ToString()
        {
            return $"{this.Id} ({this.Executable} {this.Arguments})";
        }
    }
}