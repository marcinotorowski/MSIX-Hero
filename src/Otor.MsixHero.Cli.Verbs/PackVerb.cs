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
    [Verb("pack", HelpText = "Pack a folder")]
    public class PackVerb : BaseVerb
    {
        [Option('d', "directory", Required = true)]
        public string Directory { get; set; }

        [Option('p', "package", Required = true)]
        public string Package { get; set; }

        [Option("nc", Default = false, HelpText = "Prevents MSIX Hero from compressing files in the package. By default, files in the package are compressed based on detected file type.")]
        public bool NoCompression { get; set; }

        [Option("nv", Default = false, HelpText = "Skip semantic validation. If you don't specify this option, MSIX Hero performs a full validation of the package.")]
        public bool NoValidation { get; set; }

        [Option('x', "remove", Default = false, HelpText = "If set, the input directory will be removed after packing.")]
        public bool RemoveDirectoryAfterPacking { get; set; }
    }
}
