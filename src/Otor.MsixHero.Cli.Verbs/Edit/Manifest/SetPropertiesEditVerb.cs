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

namespace Otor.MsixHero.Cli.Verbs.Edit.Manifest
{
    [Verb("setProperties", HelpText = "Sets package properties (like display name, logo, etc.)")]
    public class SetPropertiesEditVerb : BaseEditVerb
    {
        [Option("displayName", HelpText = "The display name for the package.")]
        public string DisplayName { get; set; }

        [Option("modificationPackage", HelpText = "A boolean value representing whether this is a modification package.")]
        public bool? ModificationPackage { get; set; }

        [Option("publisherDisplayName", HelpText = "The display name of the publisher.")]
        public string PublisherDisplayName { get; set; }

        [Option("logo", HelpText = "The path to a internal logo.")]
        public string Logo { get; set; }

        [Option("description", HelpText = "The package description.")]
        public string Description { get; set; }
    }
}