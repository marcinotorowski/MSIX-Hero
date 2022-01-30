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

using System.Collections.Generic;
using CommandLine;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Verbs.Resources;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("appattach", HelpText = "CLI_Verbs_AppAttach_VerbName", ResourceType = typeof(Localization))]
    public class AppAttachVerb : BaseVerb
    {
        [Option('p', "package", Min = 1, HelpText = "CLI_Verbs_AppAttach_Prop_Package", Required = true, ResourceType = typeof(Localization))]
        public IEnumerable<string> Package { get; set; }

        [Option('d', "directory", HelpText = "CLI_Verbs_AppAttach_Prop_Directory", Required = true, ResourceType = typeof(Localization))]
        public string Directory { get; set; }
        
        [Option('t', "fileType", HelpText = "CLI_Verbs_AppAttach_Prop_FileType", ResourceType = typeof(Localization))]
        public AppAttachVolumeType FileType { get; set; }

        [Option('n', "name", HelpText = "CLI_Verbs_AppAttach_Prop_Name", Required = false, ResourceType = typeof(Localization))]
        public string Name { get; set; }
        
        [Option('s', "size", HelpText = "CLI_Verbs_AppAttach_Prop_Size", Required = false, ResourceType = typeof(Localization))]
        public uint Size { get; set; }

        [Option('c', "createScripts", HelpText = "CLI_Verbs_AppAttach_Prop_CreateScript", Required = false, ResourceType = typeof(Localization))]
        public bool CreateScript { get; set; }

        [Option('e', "extractCert", HelpText = "CLI_Verbs_AppAttach_Prop_ExtractCertificate", Required = false, ResourceType = typeof(Localization))]
        public bool ExtractCertificate { get; set; }
    }
}
