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

using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Psf.Entities.Interpreter.Trace;

public class PsfTraceFixup : PsfProcessMatch
{
    public PsfTraceFixup(PsfTraceFixupConfig tracing, string processRegularExpression, string fixupName) : base(processRegularExpression, fixupName)
    {
        this.BreakOn = tracing.BreakOn;
        this.TraceMethod = tracing.TraceMethod;
        this.TraceLevels = tracing.TraceLevels;

        switch (this.TraceMethod)
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
    
    public TracingType TracingType { get; private set; }

    public PsfTraceFixupLevels TraceLevels { get; private set; }
    
    public string TraceMethod { get; private set; }
    
    public TraceLevel BreakOn { get; private set; }
}