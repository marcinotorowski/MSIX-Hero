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

using CommandLine;

namespace Otor.MsixHero.Cli.Verbs.Edit
{
    public abstract class BaseEditVerb : BaseVerb
    {
        public override string ToCommandLineString(bool withExeName = true)
        {
            if (withExeName)
            {
                return "msixherocli.exe edit " + Parser.Default.FormatCommandLine(this);
            }

            return Parser.Default.FormatCommandLine(this);
        }
    }
}