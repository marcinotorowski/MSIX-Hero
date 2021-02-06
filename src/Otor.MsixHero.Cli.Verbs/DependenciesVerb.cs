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

using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("dependencies", HelpText = "Creates a bitmap representing package dependencies")]
    public class DependenciesVerb : BaseVerb
    {
        [Option('p', "package", HelpText = "The path to the package.", Required = true)]
        public string Path { get; set; }
        
        [Option('o', "output", HelpText = "The output path (full path with extension .PNG).", Required = true)]
        public string Output { get; set; }

        [Option('w', "width", HelpText = "The width (in px).", Required = false)]
        public double? Width { get; set; }

        [Option('h', "height", HelpText = "The height (in px).", Required = false)]
        public double? Height { get; set; }
    }
}