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
    [Verb("newmodpack", HelpText = "CLI_Verbs_NewModPack_Prop_VerbName", ResourceType = typeof(Localization))]
    public class NewModPackVerb : BaseVerb
    {
        [Value(0, HelpText = "CLI_Verbs_NewModPack_Prop_OutputPath", Required = true, ResourceType = typeof(Localization))]
        public string OutputPath { get; set; }

        [Option("name", HelpText = "CLI_Verbs_NewModPack_Prop_Name", Required = true, ResourceType = typeof(Localization))]
        public string Name { get; set; }

        [Option("displayName", HelpText = "CLI_Verbs_NewModPack_Prop_DisplayName", Required = true, ResourceType = typeof(Localization))]
        public string DisplayName { get; set; }

        [Option("publisherName", HelpText = "CLI_Verbs_NewModPack_Prop_PublisherName", Required = true, ResourceType = typeof(Localization))]
        public string PublisherName { get; set; }

        [Option("publisherDisplayName", HelpText = "CLI_Verbs_NewModPack_Prop_PublisherDisplayName", Required = true, ResourceType = typeof(Localization))]
        public string PublisherDisplayName { get; set; }

        [Option("version", HelpText = "CLI_Verbs_NewModPack_Prop_Version", Required = true, ResourceType = typeof(Localization))]
        public string Version { get; set; }

        [Option("parentPath", HelpText = "CLI_Verbs_NewModPack_Prop_ParentPackagePath", SetName = "MSIX", Required = true, ResourceType = typeof(Localization))]
        public string ParentPackagePath { get; set; }

        [Option("parentName", HelpText = "CLI_Verbs_NewModPack_Prop_ParentName", SetName = "METADATA", Required = true, ResourceType = typeof(Localization))]
        public string ParentName { get; set; }

        [Option("parentPublisherName", HelpText = "CLI_Verbs_NewModPack_Prop_ParentPublisher", SetName = "METADATA", Required = true, ResourceType = typeof(Localization))]
        public string ParentPublisher { get; set; }

        [Option('r', "registry", HelpText = "CLI_Verbs_NewModPack_Prop_IncludeRegFile", ResourceType = typeof(Localization))]
        public string IncludeRegFile { get; set; }

        [Option('f', "folder", HelpText = "CLI_Verbs_NewModPack_Prop_IncludeFolder", ResourceType = typeof(Localization))]
        public string IncludeFolder { get; set; }

        [Option('c', "copyFolderStructure", HelpText = "CLI_Verbs_NewModPack_Prop_CopyFolderStructure", ResourceType = typeof(Localization))]
        public bool CopyFolderStructure { get; set; }
    }
}
