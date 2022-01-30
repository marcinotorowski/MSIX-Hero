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

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("updateImpact", HelpText = "CLI_Verbs_UpdateImpact_VerbName", ResourceType = typeof(Localization))]
    public class UpdateImpactVerb
    {
        [Option('o', "oldPackagePath", Required = true, HelpText = "CLI_Verbs_UpdateImpact_Prop_OldPackagePath", ResourceType = typeof(Localization))]
        public string OldPackagePath { get; set; }

        [Option('n', "newPackagePath", Required = true, HelpText = "CLI_Verbs_UpdateImpact_Prop_NewPackagePath", ResourceType = typeof(Localization))]
        public string NewPackagePath { get; set; }

        [Option('f', "force", HelpText = "CLI_Verbs_UpdateImpact_Prop_IgnoreVersionMismatch", ResourceType = typeof(Localization))]
        public bool IgnoreVersionMismatch { get; set; }

        [Option('x', "xml", HelpText = "CLI_Verbs_UpdateImpact_Prop_OutputXml", ResourceType = typeof(Localization))]
        public string OutputXml { get; set; }
    }
}
