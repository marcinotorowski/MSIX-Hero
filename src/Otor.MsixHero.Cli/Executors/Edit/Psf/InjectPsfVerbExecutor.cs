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

using System;
using System.Threading.Tasks;
using Otor.MsixHero.Cli.Verbs.Edit.Psf;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Psf;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Psf;

namespace Otor.MsixHero.Cli.Executors.Edit.Psf
{
    public class InjectPsfVerbExecutor : BaseEditVerbExecutor<InjectPsfEditVerb>
    {
        public InjectPsfVerbExecutor(string package, InjectPsfEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            var action = new InjectPsf
            {
                ApplicationIds = this.Verb.ApplicationIds,
                FileNames = this.Verb.FileNames,
                Force = this.Verb.Force
            };

            var executor = new InjectPsfExecutor(directoryPath);
            try
            {
                await executor.Execute(action).ConfigureAwait(false);
                await this.Console.WriteSuccess("OK").ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}
