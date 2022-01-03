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
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry;
using Otor.MsixHero.Cli.Verbs.Edit.Registry;

namespace Otor.MsixHero.Cli.Executors.Edit.Registry
{
    public class ImportRegistryVerbExecutor : BaseEditVerbExecutor<ImportRegistryEditVerb>
    {
        public ImportRegistryVerbExecutor(string package, ImportRegistryEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            return this.Verb.FilePath != null 
                ? this.ImportRegFileToExtractedPackage(directoryPath) 
                : this.ImportKeyToExtractedPackage(directoryPath);
        }

        protected override async Task<int> Validate()
        {
            if (!string.IsNullOrEmpty(this.Verb.FilePath))
            {
                if (!File.Exists(this.Verb.FilePath))
                {
                    await this.Console.WriteError($"Registry file {this.Verb.FilePath} does not exist.");
                    return 10;
                }

                return await base.Validate().ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(this.Verb.RegistryKey))
            {
                if (this.Verb.RegistryKey.IndexOf('\\') == -1)
                {
                    await this.Console.WriteError("Registry key must be a path starting with a valid hive name.").ConfigureAwait(false);
                    return 10;
                }

                var hiveName = this.Verb.RegistryKey.Substring(0, this.Verb.RegistryKey.IndexOf('\\'));
                switch (hiveName.ToLowerInvariant())
                {
                    case "hkcr":
                    case "hklm":
                    case "hkcu":
                    case "hkey_local_machine":
                    case "hkey_current_user":
                    case "hkey_classes_root":
                    case "machine":
                    case "user":
                    case "classes":
                        return await base.Validate().ConfigureAwait(false);
                }

                await this.Console.WriteError("Registry hive " + hiveName + " is not supported.").ConfigureAwait(false);
                return 10;
            }

            await this.Console.WriteError("Either the file path or the local registry path must be provided.").ConfigureAwait(false);
            return 11;
        }
        
        private async Task<int> ImportKeyToExtractedPackage(string directoryPath)
        {
            var command = new ImportLocalRegistryKey
            {
                RegistryKey = this.Verb.RegistryKey
            };

            try
            {
                await new ImportLocalRegistryKeyExecutor(directoryPath).Execute(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError($"Could not import registry key {this.Verb.RegistryKey}.").ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            await this.Console.WriteSuccess($"Registry key '{this.Verb.RegistryKey}' has been successfully imported.").ConfigureAwait(false);
            return StandardExitCodes.ErrorSuccess;
        }

        private async Task<int> ImportRegFileToExtractedPackage(string directoryPath)
        {
            var command = new ImportRegistryFile
            {
                FilePath = this.Verb.FilePath
            };

            try
            {
                await new ImportRegistryFileExecutor(directoryPath).Execute(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError($"Registry file {this.Verb.FilePath} could not be imported.").ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            await this.Console.WriteSuccess($"Registry file '{this.Verb.FilePath}' has been successfully imported.").ConfigureAwait(false);
            return StandardExitCodes.ErrorSuccess;
        }
    }
}
