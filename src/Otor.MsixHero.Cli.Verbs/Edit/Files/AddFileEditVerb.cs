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
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Files
{
    [Verb("addFile", HelpText = "CLI_Verbs_Edit_AddFile_VerbName", ResourceType = typeof(Localization))]
    public class AddFileEditVerb : BaseEditVerb
    {
        [Option('t', "target", HelpText = "CLI_Verbs_Edit_AddFile_Prop_Target", Required = true, ResourceType = typeof(Localization))]
        public string DestinationPath { get; set; }

        [Option('s', "source", HelpText = "CLI_Verbs_Edit_AddFile_Prop_Source", Required = true, ResourceType = typeof(Localization))]
        public string SourcePath { get; set; }

        [Option('f', "force", HelpText = "CLI_Verbs_Edit_AddFile_Prop_Force", ResourceType = typeof(Localization))]
        public bool Force { get; set; }
    }
}