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

namespace Otor.MsixHero.Cli.Verbs.Edit.Files
{
    [Verb("deleteFile", HelpText = "CLI_Verbs_Edit_DeleteFile_VerbName", ResourceType = typeof(Localization))]
    public class DeleteFileEditVerb : BaseEditVerb
    {
        [Option('f', "file", HelpText = "CLI_Verbs_Edit_DeleteFile_Prop_FilePath", Required = true, ResourceType = typeof(Localization))]
        public string FilePath { get; set; }
    }
}
