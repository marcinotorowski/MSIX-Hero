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

namespace Otor.MsixHero.Cli.Verbs.Edit.Manifest
{
    [Verb("setBuildMetaData", HelpText = "Adds a specified build meta data to the package manifest.")]
    public class SetBuildMetaDataVerb : BaseEditVerb
    {
        [Value(0, HelpText = "The name of the component, for example SignTool.exe.")]
        public string Name { get; set; }

        [Value(1, HelpText = "The version of the component, for example 1.0.0.")]
        public string Version { get; set; }
    }
}