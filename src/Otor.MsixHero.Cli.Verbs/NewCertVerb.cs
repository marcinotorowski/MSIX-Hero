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

using System;
using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("newcert", HelpText = "Create new certificate for self-signing.")]
    public class NewCertVerb : BaseVerb
    {
        [Option('n', "name", HelpText = "Certificate name", Required = true)]
        public string DisplayName { get; set; }

        [Option('s', "subject", HelpText = "Certificate subject, for example CN=John. If not provided, it will be set automatically from the display name.")]
        public string Subject { get; set; }

        [Option('d', "directory", HelpText = "Directory, where certificate files will be saved.", Required = true)]
        public string OutputFolder { get; set; }

        [Option("validUntil", HelpText = "Date time until which the certificate can be used for signing purposes. Defaults to one year from today.", Required = false)]
        public DateTime? ValidUntil { get; set; }

        [Option('i', "import", HelpText = "If set, the certificate will be imported to the local store. This option requires that MSIXHeroCLI.exe is started as administrator.")]
        public bool Import { get; set; }

        [Option('p', "password", HelpText = "Certificate password.", Required = false)]
        public string Password { get; set; }
    }
}
