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

namespace Otor.MsixHero.Appx.Psf.Entities.Interpreter;

public abstract class PsfProcessMatch
{
    protected PsfProcessMatch(string processRegularExpression, InterpretedPsfBitness psfInterpretedBitness)
    {
        this.PsfInterpretedBitness = psfInterpretedBitness;
        this.RegularExpression = InterpretedRegex.CreateInterpretedRegex(processRegularExpression);
    }

    protected PsfProcessMatch(string processRegularExpression, string fixupName)
    {
        this.RegularExpression = InterpretedRegex.CreateInterpretedRegex(processRegularExpression);
        this.FixupName = fixupName;

        if (fixupName.IndexOf("64", StringComparison.Ordinal) != -1)
        {
            this.PsfInterpretedBitness = InterpretedPsfBitness.x64;
        }
        else if (fixupName.IndexOf("32", StringComparison.Ordinal) != -1 || fixupName.IndexOf("86", StringComparison.Ordinal) != -1)
        {
            this.PsfInterpretedBitness = InterpretedPsfBitness.x86;
        }
        else
        {
            this.PsfInterpretedBitness = InterpretedPsfBitness.Unknown;
        }
    }

    public InterpretedPsfBitness PsfInterpretedBitness { get; private set; }

    public InterpretedRegex RegularExpression { get; private set; }

    public string FixupName { get; private set; }
}