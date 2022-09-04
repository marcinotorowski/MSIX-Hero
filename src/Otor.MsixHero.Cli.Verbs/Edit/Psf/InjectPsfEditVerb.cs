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

using System.Collections.Generic;
using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs.Edit.Psf
{
    [Verb("injectPsf", HelpText = "CLI_Verbs_Edit_InjectPsf_VerbName", ResourceType = typeof(Localization))]
    public class InjectPsfEditVerb : BaseEditVerb
    {
        [Option('a', "applicationIds", SetName = "APPID", HelpText = "CLI_Verbs_Edit_InjectPsf_ApplicationIds", ResourceType = typeof(Localization))]
        public List<string> ApplicationIds { get; set; }

        [Option('n', "fileNames", Group = "FILENAMES", HelpText = "CLI_Verbs_Edit_InjectPsf_FileNames", ResourceType = typeof(Localization))]
        public List<string> FileNames { get; set; }

        [Option('f', "force", Default = false, HelpText = "CLI_Verbs_Edit_InjectPsf_Force", ResourceType = typeof(Localization))]
        public bool Force { get; set; }

        [Option('s', "launcherSourcePath", Default = false, HelpText = "CLI_Verbs_Edit_InjectPsf_PsfLauncherSourcePath", ResourceType = typeof(Localization))]
        public string PsfLauncherSourcePath { get; set; }
    }
}
