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
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Cli.Verbs.Edit.Manifest;

namespace Otor.MsixHero.Cli.Executors.Edit.Manifest
{
    public class SetBuildMetaDataVerbExecutor : ManifestEditVerbExecutor<SetBuildMetaDataVerb>
    {
        public SetBuildMetaDataVerbExecutor(string package, SetBuildMetaDataVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> Validate()
        {
            var baseValidation = await base.Validate().ConfigureAwait(false);
            if (baseValidation != StandardExitCodes.ErrorSuccess)
            {
                return baseValidation;
            }

            if (string.IsNullOrEmpty(this.Verb.Name))
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_BuildMetaData_Error_EmptyName).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (string.IsNullOrEmpty(this.Verb.Version))
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_BuildMetaData_Error_EmptyVersion).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            return StandardExitCodes.ErrorSuccess;
        }
        
        protected override async Task<int> ExecuteOnManifest(XDocument document)
        {
            var action = new SetBuildMetaData(this.Verb.Name, this.Verb.Version);
            
            var executor = new SetBuildMetaDataExecutor(document);
            executor.ValueChanged += (_, change) =>
            {
                if (string.IsNullOrEmpty(change.OldValue) || string.Equals(change.OldValue, change.NewValue, StringComparison.Ordinal))
                {
                    this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_BuildMetaData_Success_Set_Format, change.Key, change.NewValue)).GetAwaiter().GetResult();
                }
                else
                {
                    this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_BuildMetaData_Success_Change_Format, change.Key, change.OldValue, change.NewValue)).GetAwaiter().GetResult();
                }
            };

            try
            {
                await executor.Execute(action).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_BuildMetaData_Error_Format, this.Verb.Name)).ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}