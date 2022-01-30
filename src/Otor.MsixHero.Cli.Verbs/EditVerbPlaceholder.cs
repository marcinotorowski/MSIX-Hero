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

using Otor.MsixHero.Cli.Verbs.Resources;
using CommandLine;

namespace Otor.MsixHero.Cli.Verbs
{
    [Verb("edit", HelpText = "CLI_Verbs_Edit_VerbName", ResourceType = typeof(Localization))]
    public class EditVerbPlaceholder : BaseVerb
    {
        [Value(0, HelpText = "CLI_Verbs_Edit_Prop_PackagePath", Required = true, ResourceType = typeof(Localization))]
        public string PackagePath { get; set; }
    }
}