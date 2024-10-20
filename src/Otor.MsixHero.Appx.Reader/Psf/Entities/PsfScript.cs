﻿// MSIX Hero
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

using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities
{
    [DataContract]
    public class PsfScript : JsonElement
    {
        /// <summary>
        /// The path to the script including the name and extension.The path starts from the root directory of the application.
        /// </summary>
        [DataMember(Name = "scriptPath")]
        public string ScriptPath { get; set; }

        /// <summary>
        /// Space delimited argument list.The format is the same for a PowerShell script call. This string gets appended to scriptPath to make a valid PowerShell.exe call.
        /// </summary>
        [DataMember(Name = "scriptArguments")]
        public string ScriptArguments { get; set; }

        /// <summary>
        /// Specifies whether the script should run in the same virtual environment that the packaged application runs in.
        /// </summary>
        [DataMember(Name = "runInVirtualEnvironment")]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool RunInVirtualEnvironment { get; set; }

        /// <summary>
        /// Specifies whether the script should run once per user, per version.
        /// </summary>
        [DataMember(Name = "runOnce")]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool RunOnce { get; set; }

        /// <summary>
        /// Specifies whether the packaged application should wait for the starting script to finish before starting.
        /// </summary>
        [DataMember(Name = "waitForScriptToFinish")]
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool WaitForScriptToFinish { get; set; }

        /// <summary>
        /// Specifies whether the PowerShell window is shown.
        /// </summary>
        [DataMember(Name = "showWindow")]
        public bool ShowWindow { get; set; }

        /// <summary>
        /// How long the script will be allowed to execute. When the time elapses, the script will be stopped.
        /// </summary>
        [DataMember(Name = "timeout")]
        public uint Timeout { get; set; }
    }
}