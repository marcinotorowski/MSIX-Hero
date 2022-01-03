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
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Files;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Cli.Verbs.Edit.Files;

namespace Otor.MsixHero.Cli.Executors.Edit.Files
{
    public class AddFileEditVerbExecutor : BaseEditVerbExecutor<AddFileEditVerb>
    {
        public AddFileEditVerbExecutor(string package, AddFileEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> Validate()
        {
            if (!File.Exists(this.Verb.SourcePath))
            {
                await this.Console.WriteError($"File ('{this.Verb.SourcePath}') does not exist.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (string.Equals(FileConstants.AppxManifestFile, this.Verb.DestinationPath))
            {
                await this.Console.WriteError("Manifest file cannot be added directly.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            return await base.Validate().ConfigureAwait(false);
        }

        protected override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            var action = new AddFile
            {
                DestinationPath = this.Verb.DestinationPath,
                SourcePath = this.Verb.SourcePath,
                Force = this.Verb.Force
            };

            var executor = new AddFileExecutor(directoryPath);
            try
            {
                await executor.Execute(action).ConfigureAwait(false);
                await this.Console.WriteSuccess($"File '{this.Verb.SourcePath}' has been imported into '{this.Verb.DestinationPath}'.").ConfigureAwait(false);
            }
            catch (AddFileExecutor.FileAlreadyExistsException alreadyExistsException)
            {
                await this.Console.WriteError($"File '{this.Verb.SourcePath}' could not be added because the destination file '{alreadyExistsException.FilePath}' already exists.").ConfigureAwait(false);
                await this.Console.WriteInfo("You can force replacement of the file by using --force switch.").ConfigureAwait(false);
                return StandardExitCodes.ErrorResourceExists;
            }
            catch (Exception e)
            {
                await this.Console.WriteError($"File '{this.Verb.SourcePath}' could not be added to '{this.Verb.DestinationPath}'.").ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}
