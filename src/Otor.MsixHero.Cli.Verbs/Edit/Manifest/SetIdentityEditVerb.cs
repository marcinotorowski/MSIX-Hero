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

namespace Otor.MsixHero.Cli.Verbs.Edit.Manifest
{
    [Verb("setIdentity", HelpText = "CLI_Verbs_Edit_SetIdentity_VerbName", ResourceType = typeof(Localization))]
    public class SetIdentityEditVerb : BaseEditVerb
    {
        [Option("publisher", HelpText = "CLI_Verbs_Edit_SetIdentity_Prop_Publisher", ResourceType = typeof(Localization))]
        public string Publisher { get; set; }

        [Option("packageVersion", HelpText = "CLI_Verbs_Edit_SetIdentity_Prop_Version", ResourceType = typeof(Localization))]
        public string Version { get; set; }

        [Option("packageName", HelpText = "CLI_Verbs_Edit_SetIdentity_Prop_Name", ResourceType = typeof(Localization))]
        public string Name { get; set; }

        [Option("resourceId", HelpText = "CLI_Verbs_Edit_SetIdentity_Prop_ResourceId", ResourceType = typeof(Localization))]
        public string ResourceId { get; set; }

        [Option("processorArchitecture", HelpText = "CLI_Verbs_Edit_SetIdentity_Prop_ProcessorArchitecture", ResourceType = typeof(Localization))]
        public string ProcessorArchitecture { get; set; }
    }
}
