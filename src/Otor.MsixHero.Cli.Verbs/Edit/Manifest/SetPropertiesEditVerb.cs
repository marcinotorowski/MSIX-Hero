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
    [Verb("setProperties", HelpText = "CLI_Verbs_Edit_SetProperties_VerbName", ResourceType = typeof(Localization))]
    public class SetPropertiesEditVerb : BaseEditVerb
    {
        [Option("displayName", HelpText = "CLI_Verbs_Edit_SetProperties_Prop_DisplayName", ResourceType = typeof(Localization))]
        public string DisplayName { get; set; }

        [Option("modificationPackage", HelpText = "CLI_Verbs_Edit_SetProperties_Prop_ModificationPackage", ResourceType = typeof(Localization))]
        public bool? ModificationPackage { get; set; }

        [Option("publisherDisplayName", HelpText = "CLI_Verbs_Edit_SetProperties_Prop_PublisherDisplayName", ResourceType = typeof(Localization))]
        public string PublisherDisplayName { get; set; }

        [Option("logo", HelpText = "CLI_Verbs_Edit_SetProperties_Prop_Logo", ResourceType = typeof(Localization))]
        public string Logo { get; set; }

        [Option("description", HelpText = "CLI_Verbs_Edit_SetProperties_Prop_Description", ResourceType = typeof(Localization))]
        public string Description { get; set; }
    }
}