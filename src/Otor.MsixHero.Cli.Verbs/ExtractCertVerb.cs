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
    [Verb("extract-cert", HelpText = "Extracts the certificate file (.CER) from an already signed MSIX package.")]
    public class ExtractCertVerb : BaseVerb
    {
        [Value(1, Required = true)]
        public string File { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "Full file path under which the extracted .CER file will be saved.")]
        public string Output { get; set; }
    }
}