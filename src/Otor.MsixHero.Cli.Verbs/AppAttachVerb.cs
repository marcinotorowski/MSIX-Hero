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

using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("appattach", HelpText = "Creates VHD disk fro app attach.")]
    public class AppAttachVerb : BaseVerb
    {
        [Option('p', "package", HelpText = "Full path to MSIX package to be converted to VHD.", Required = true)]
        public string Package { get; set; }

        [Option('d', "directory", HelpText = "Output directory folder.", Required = true)]
        public string Directory { get; set; }

        [Option('n', "name", HelpText = "File name for VHD output. If left empty, the MSIX file name without extension will be used.", Required = false)]
        public string Name { get; set; }
        
        [Option('s', "size", HelpText = "Size of VHD container (in MB). Leave empty for auto-selection.", Required = false)]
        public uint Size { get; set; }

        [Option('c', "createScripts", HelpText = "If specified, sample PS1 scripts for registration and de-registration will be created.", Required = false)]
        public bool CreateScript { get; set; }

        [Option('e', "extractCert", HelpText = "If specified, digital certificate will be extracted from the specified package.", Required = false)]
        public bool ExtractCertificate { get; set; }
    }
}
