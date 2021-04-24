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
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("appattach", HelpText = "Creates app attach volume for MSIX packages.")]
    public class AppAttachVerb : BaseVerb
    {
        [Option('p', "package", Min = 1, HelpText = "Full paths to MSIX packages to be converted to app attach format. If the path is a directory, then all MSIX from the folder will be converted.", Required = true)]
        public IEnumerable<string> Package { get; set; }

        [Option('d', "directory", HelpText = "Output directory folder.", Required = true)]
        public string Directory { get; set; }
        
        [Option('t', "fileType", HelpText = "The type of the output file.")]
        public AppAttachVolumeType FileType { get; set; }

        [Option('n', "name", HelpText = "File name for the output file. If left empty, the MSIX file name without extension will be used. This parameter is not used in case of multiple input packages.", Required = false)]
        public string Name { get; set; }
        
        [Option('s', "size", HelpText = "Size of the container (in MB). Leave empty for auto-selection.", Required = false)]
        public uint Size { get; set; }

        [Option('c', "createScripts", HelpText = "If specified, sample PS1 scripts for registration and de-registration will be created. This is currently supported only for VHD volumes.", Required = false)]
        public bool CreateScript { get; set; }

        [Option('e', "extractCert", HelpText = "If specified, digital certificate will be extracted from the specified package.", Required = false)]
        public bool ExtractCertificate { get; set; }
    }
}
