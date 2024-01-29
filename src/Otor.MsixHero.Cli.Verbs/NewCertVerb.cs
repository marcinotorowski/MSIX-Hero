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

using System;
using Otor.MsixHero.Cli.Verbs.Resources;
using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("newcert", HelpText = "CLI_Verbs_NewCert_VerbName", ResourceType = typeof(Localization))]
    public class NewCertVerb : BaseVerb
    {
        [Option('n', "name", HelpText = "CLI_Verbs_NewCert_Prop_DisplayName", Required = true, ResourceType = typeof(Localization))]
        public string DisplayName { get; set; }

        [Option('s', "subject", HelpText = "CLI_Verbs_NewCert_Prop_Subject", ResourceType = typeof(Localization))]
        public string Subject { get; set; }

        [Option('d', "directory", HelpText = "CLI_Verbs_NewCert_Prop_OutputFolder", Required = true, ResourceType = typeof(Localization))]
        public string OutputFolder { get; set; }

        [Option("validUntil", HelpText = "CLI_Verbs_NewCert_Prop_ValidUntil", Required = false, ResourceType = typeof(Localization))]
        public DateTime? ValidUntil { get; set; }

        [Option('i', "import", HelpText = "CLI_Verbs_NewCert_Prop_Import", ResourceType = typeof(Localization))]
        public bool Import { get; set; }

        [Option('p', "password", HelpText = "CLI_Verbs_NewCert_Prop_Password", Required = false, ResourceType = typeof(Localization))]
        public string Password { get; set; }
    }
}
