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

namespace Otor.MsixHero.Cli.Verbs.Edit.Files
{
    [Verb("addFile", HelpText = "Adds or replaces a specific file in the package.")]
    public class AddFileEditVerb : BaseEditVerb
    {
        [Option('t', "target", HelpText = "Target relative path to current file.", Required = true)]
        public string DestinationPath { get; set; }

        [Option('s', "source", HelpText = "Source file from disk.", Required = true)]
        public string SourcePath { get; set; }

        [Option('f', "force", HelpText = "If set then in case the target file exists it will be overridden.")]
        public bool Force { get; set; }
    }
}