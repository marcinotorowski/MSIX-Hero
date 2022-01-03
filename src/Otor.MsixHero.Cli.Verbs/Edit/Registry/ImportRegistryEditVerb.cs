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

namespace Otor.MsixHero.Cli.Verbs.Edit.Registry
{
    [Verb("importRegistry", HelpText = "Imports registry from .reg file or local Registry.")]
    public class ImportRegistryEditVerb : BaseEditVerb, IBaseEditRegistryVerb
    {
        [Option('f', "file", HelpText = "A .reg file (Windows Registry) to be imported.", Required = true, SetName = "Import from .REG file")]
        public string FilePath { get; set; }

        [Option('k', "key", HelpText = "A local registry path (for example HKLM\\Software\\abc) of a registry key to be imported.", Required = true, SetName = "Import from local registry key")]
        public string RegistryKey { get; set; }
    }
}