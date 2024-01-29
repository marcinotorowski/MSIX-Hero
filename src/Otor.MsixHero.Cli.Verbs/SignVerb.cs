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

using System.Collections.Generic;
using CommandLine;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("sign", HelpText = "CLI_Verbs_Sign_VerbName", ResourceType = typeof(Localization))]
    public class SignVerb : BaseVerb
    {
        [Value(1, MetaName = "file path", HelpText = "CLI_Verbs_Sign_Prop_FilePath", ResourceType = typeof(Localization))]
        public IEnumerable<string> FilePath { get; set; }

        [Option("sm", HelpText = "CLI_Verbs_Sign_Prop_UseMachineStore", ResourceType = typeof(Localization), Default = false, Required = false, SetName = "Installed")]
        public bool UseMachineStore { get; set; }

        [Option('t', "timestamp", HelpText = "CLI_Verbs_Sign_Prop_TimeStampUrl", ResourceType = typeof(Localization), Required = false)]
        public string TimeStampUrl { get; set; }
        
        [Option("sha1", HelpText = "CLI_Verbs_Sign_Prop_ThumbPrint", ResourceType = typeof(Localization), Required = false, SetName = "Installed")]
        public string ThumbPrint { get; set; }

        [Option('f', "file", HelpText = "CLI_Verbs_Sign_Prop_PfxFilePath", ResourceType = typeof(Localization), Required = false, SetName = "PFX")]
        public string PfxFilePath { get; set; }
        
        [Option('p', "password", HelpText = "CLI_Verbs_Sign_Prop_PfxPassword", ResourceType = typeof(Localization), Required = false, SetName = "PFX")]
        public string PfxPassword { get; set; }

        [Option("dgf", HelpText = "CLI_Verbs_Sign_Prop_DeviceGuardFile", ResourceType = typeof(Localization), SetName = "DeviceGuard")]
        public string DeviceGuardFile { get; set; }

        [Option("dg", HelpText = "CLI_Verbs_Sign_Prop_DeviceGuardInteractive", ResourceType = typeof(Localization), SetName = "DeviceGuard")]
        public bool DeviceGuardInteractive { get; set; }
        
        [Option("dgp", HelpText = "CLI_Verbs_Sign_Prop_DeviceGuardSubject", ResourceType = typeof(Localization), SetName = "DeviceGuard")]
        public string DeviceGuardSubject { get; set; }

        [Option('i', "increaseVersion", HelpText = "CLI_Verbs_Sign_Prop_IncreaseVersion", ResourceType = typeof(Localization), Default = IncreaseVersionMethod.None, Required = false)]
        public IncreaseVersionMethod IncreaseVersion { get; set; }

        [Option("noPublisherUpdate", HelpText = "CLI_Verbs_Sign_Prop_NoPublisherUpdate", ResourceType = typeof(Localization), Default = false)]
        public bool NoPublisherUpdate { get; set; }
    }
}
