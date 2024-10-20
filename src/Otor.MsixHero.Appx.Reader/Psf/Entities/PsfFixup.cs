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

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities
{
    [DataContract]
    public class PsfFixup : JsonElement
    {
        [DataMember(Name = "dll")]
        public string Dll { get; set; }

        [JsonIgnore]
        public PsfFixupConfig Config { get; set; }
        
        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            if (this.Config == null)
            {
                this.rawValues = null;
                return;
            }

            this.rawValues = JObject.FromObject(this.Config);
        }

        [OnDeserialized]
        internal void OnDeserializingMethod(StreamingContext context)
        {
            if (!this.rawValues.ContainsKey("config"))
            {
                return;
            }

            if (this.Dll?.StartsWith("FileRedirectionFixup", StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Config = JsonConvert.DeserializeObject<PsfRedirectionFixupConfig>(this.rawValues["config"].ToString(Formatting.None));
            }
            else if (this.Dll?.StartsWith("TraceFixup", StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Config = JsonConvert.DeserializeObject<PsfTraceFixupConfig>(this.rawValues["config"].ToString(Formatting.None));
            }
            else if (this.Dll?.StartsWith("ElectronFixup", StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Config = JsonConvert.DeserializeObject<PsfElectronFixupConfig>(this.rawValues["config"].ToString(Formatting.None));
            }
            else
            {
                this.Config = new CustomPsfFixupConfig(this.rawValues["config"].ToString(Formatting.Indented));
            }

            this.rawValues.Remove("config");
        }

        public override string ToString()
        {
            return this.Dll;
        }
    }
}