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
    public class PsfTracingRedirectionDescriptor
    {
        private readonly PsfTraceFixupConfig traceFixupConfig;

        // For serialization
        public PsfTracingRedirectionDescriptor()
        {
        }

        public PsfTracingRedirectionDescriptor(PsfTraceFixupConfig traceFixupConfig, PsfBitness psfBitness)
        {
            PsfBitness = psfBitness;
            this.traceFixupConfig = traceFixupConfig;

            var methods = traceFixupConfig.TraceMethod?.ToLowerInvariant();
            this.BreakOn = traceFixupConfig.BreakOn;


            if (string.IsNullOrEmpty(methods))
            {
                return;
            }

            switch (methods)
            {
                case "printf":
                    this.TracingType = TracingType.Console;
                    break;
                case "eventlog":
                    this.TracingType = TracingType.EventLog;
                    break;
                default:
                    this.TracingType = TracingType.Default;
                    break;
            }
        }

        [DataMember(Name = "psfBitness")]
        public PsfBitness PsfBitness { get; set; }

        [DataMember(Name = "tracingType")]
        public TracingType TracingType { get; set; }


        [DataMember(Name = "breakOn")]
        public TraceLevel BreakOn { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
