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

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("unpack", HelpText = "Unpack a folder")]
    public class UnpackVerb : BaseVerb
    {
        [Option('p', "package", Required = true)]
        public string Package { get; set; }

        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }

        [Option("nv", Default = false, HelpText = "Skip semantic validation. If you don't specify this option, MSIX Hero performs a full validation of the package.")]
        public bool NoValidation { get; set; }

        [Option('x', "remove", Default = false, HelpText = "If set, the input package will be removed after packing.")]
        public bool RemovePackageAfterExtraction { get; set; }
    }
}