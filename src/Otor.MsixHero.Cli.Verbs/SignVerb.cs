// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("sign", HelpText = "Sign a package")]
    public class SignVerb : BaseVerb
    {
        [Value(1, MetaName = "file path", HelpText = "Full paths to one or more files (separated by space).")]
        public IEnumerable<string> FilePath { get; set; }

        [Option("sm", HelpText = "Specifies that a machine store, instead of a user store, is used.", Default = false, Required = false, SetName = "Installed certificate")]
        public bool UseMachineStore { get; set; }

        [Option('t', "timestamp", HelpText = "Specifies the URL of the RFC 3161 time stamp server. If not specified, the default value from MSIX Hero settings will be used.", Required = false)]
        public string TimeStampUrl { get; set; }
        
        [Option("sha1", HelpText = "Specifies the SHA1 hash of the signing certificate. The SHA1 hash is commonly specified when multiple certificates satisfy the criteria specified by the remaining switches.", Required = false, SetName = "Installed certificate")]
        public string ThumbPrint { get; set; }

        [Option('f', "file", HelpText = "Specifies the signing certificate in a file. If the file is in Personal Information Exchange (PFX) format and protected by a password, use the -p option to specify the password.", Required = false, SetName = "PFX")]
        public string PfxFilePath { get; set; }
        
        [Option('p', "password", HelpText = "	Specifies the password to use when opening a PFX file.", Required = false, SetName = "PFX")]
        public string PfxPassword { get; set; }

        [Option("dgf", HelpText = "Full path to JSON file containing access tokens to AzureAD.", SetName = "Device Guard Signing")]
        public string DeviceGuardFile { get; set; }

        [Option("dg", HelpText = "A switch for interactive Device Guard signing (you will be asked for AzureAD credentials).", SetName = "Device Guard Signing")]
        public bool DeviceGuardInteractive { get; set; }
        
        [Option("dgp", HelpText = "Publisher name used for signing with Device Guard.", SetName = "Device Guard Signing")]
        public string DeviceGuardSubject { get; set; }

        [Option('i', "increaseVersion", HelpText = "Specifies whether the version should be automatically increased, and (if yes) which component of it. Supported values are [None, Major, Minor, Build, Revision].", Default = IncreaseVersionMethod.None, Required = false)]
        public IncreaseVersionMethod IncreaseVersion { get; set; }

        [Option("noPublisherUpdate", HelpText = "Disables the automatic update of publisher name with the value of certificate subject.", Default = false)]
        public bool NoPublisherUpdate { get; set; }
    }
}
