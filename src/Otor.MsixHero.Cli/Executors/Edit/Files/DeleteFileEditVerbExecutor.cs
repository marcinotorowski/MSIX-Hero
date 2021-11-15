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
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Files;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files;
using Otor.MsixHero.Cli.Verbs.Edit.Files;

namespace Otor.MsixHero.Cli.Executors.Edit.Files
{
    public class DeleteFileEditVerbExecutor : BaseEditVerbExecutor<DeleteFileEditVerb>
    {
        public DeleteFileEditVerbExecutor(string package, DeleteFileEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            var action = new DeleteFile
            {
                FilePath = this.Verb.FilePath
            };

            var executor = new DeleteFileExecutor(directoryPath);
            try
            {
                executor.OnFileRemoved += (_, filePath) =>
                {
                    this.Console.WriteSuccess($"File '{filePath}' has been removed.").GetAwaiter().GetResult();
                };

                await executor.Execute(action).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError($"File '{this.Verb.FilePath}' could not be removed.").ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}
