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

using CommandLine;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("pack", HelpText = "CLI_Verbs_Pack_VerbName", ResourceType = typeof(Localization))]
    public class PackVerb : BaseVerb
    {
        [Option('d', "directory", Required = true, HelpText = "CLI_Verbs_Pack_Prop_Directory", ResourceType = typeof(Localization))]
        public string Directory { get; set; }

        [Option('p', "package", Required = true, HelpText = "CLI_Verbs_Pack_Prop_Package", ResourceType = typeof(Localization))]
        public string Package { get; set; }

        [Option("nc", Default = false, HelpText = "CLI_Verbs_Pack_Prop_NoCompression", ResourceType = typeof(Localization))]
        public bool NoCompression { get; set; }

        [Option("nv", Default = false, HelpText = "CLI_Verbs_Pack_Prop_NoValidation", ResourceType = typeof(Localization))]
        public bool NoValidation { get; set; }

        [Option('x', "remove", Default = false, HelpText = "CLI_Verbs_Pack_Prop_RemoveDirectoryAfterPacking", ResourceType = typeof(Localization))]
        public bool RemoveDirectoryAfterPacking { get; set; }
    }
}
