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

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public class PsfTraceFixupConfig : PsfFixupConfig
    {
        [DataMember(Name = "traceMethod")]
        [DefaultValue("default")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string TraceMethod { get; set; }
        
        [DataMember(Name = "traceLevels")]
        public PsfTraceFixupLevels TraceLevels { get; set; }
        
        [DataMember(Name = "breakOn")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(TraceLevel.UnexpectedFailures)]
        public TraceLevel BreakOn { get; set; }

        public override string ToString()
        {
            return $"Trace method {this.TraceMethod}, break on {this.BreakOn}";
        }
    }
}