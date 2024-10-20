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

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor
{
    [Serializable]
    public class PsfScriptDescriptor
    {
        private readonly PsfScript script;

        // For serialization
        public PsfScriptDescriptor()
        {
        }

        public PsfScriptDescriptor(PsfScript script, PsfScriptDescriptorTiming timing)
        {
            this.script = script;
            this.Timing = timing;
            this.Name = this.script.ScriptPath;
            this.Arguments = this.script.ScriptArguments;
            this.RunInVirtualEnvironment = this.script.RunInVirtualEnvironment;
            this.RunOnce = this.script.RunOnce;
            this.ShowWindow = this.script.ShowWindow;
            this.WaitForScriptToFinish = this.script.WaitForScriptToFinish;
        }

        [DataMember(Name = "scriptPath")]
        public string Name { get; set; }

        [DataMember(Name = "scriptArguments")]
        public string Arguments { get; set; }

        [DataMember(Name = "timing")]
        public PsfScriptDescriptorTiming Timing { get; set; }

        [DataMember(Name = "runOnce")]
        public bool RunOnce { get; set; }

        [DataMember(Name = "runInVirtualEnvironment")]
        public bool RunInVirtualEnvironment { get; set; }

        [DataMember(Name = "waitForScriptToFinish")]
        public bool WaitForScriptToFinish { get; set; }
        
        [DataMember(Name = "showWindow")]
        public bool ShowWindow { get; set; }
    }
}