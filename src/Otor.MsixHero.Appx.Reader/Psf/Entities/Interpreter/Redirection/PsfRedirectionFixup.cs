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
using System.Linq;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Redirection;

public class PsfRedirectionFixup : PsfProcessMatch
{
    public PsfRedirectionFixup(IEnumerable<PsfRedirectionRule> rules, string processRegularExpression, string fixupName) : base(processRegularExpression, fixupName)
    {
        this.Rules = rules?.ToList() ?? new List<PsfRedirectionRule>();
    }
    
    public IReadOnlyList<PsfRedirectionRule> Rules { get; private set; }
}