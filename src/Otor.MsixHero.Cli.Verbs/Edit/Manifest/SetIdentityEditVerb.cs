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
    [Verb("setIdentity", HelpText = "Sets package identity.")]
    public class SetIdentityEditVerb : BaseEditVerb
    {
        [Option("publisher", HelpText = "The CN string of a publisher.")]
        public string Publisher { get; set; }

        [Option("packageVersion", HelpText = "The package version. It must be a valid version string. You can also write 'auto' to autoincrement the version, or use placeholders ^ or + to increase a unit, and 'x' or '*' to keep the unit. For example, the version *.*.+.0 would keep the major and minor version from the current value, increase the third position by one, and reset the fourth position to 0.")]
        public string Version { get; set; }

        [Option("packageName", HelpText = "The package name.")]
        public string Name { get; set; }

        [Option("processorArchitecture", HelpText = "The processor architecture, for example x64, arm or Neutral")]
        public string ProcessorArchitecture { get; set; }
    }
}
