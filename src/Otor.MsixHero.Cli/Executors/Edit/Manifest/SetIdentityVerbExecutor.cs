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

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Cli.Verbs.Edit.Manifest;

namespace Otor.MsixHero.Cli.Executors.Edit.Manifest
{
    public class SetIdentityVerbExecutor : ManifestEditVerbExecutor<SetIdentityEditVerb>
    {
        public SetIdentityVerbExecutor(string package, SetIdentityEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> ExecuteOnManifest(XDocument document)
        {
            var action = new SetPackageIdentity
            {
                Name = this.Verb.Name,
                ProcessorArchitecture = this.Verb.ProcessorArchitecture,
                Publisher = this.Verb.Publisher,
                Version = this.Verb.Version,
                ResourceId = this.Verb.ResourceId
            };

            var actionExecutor = new SetPackageIdentityExecutor(document);
            actionExecutor.ValueChanged += (_, changed) =>
            {
                if (string.IsNullOrEmpty(changed.OldValue) || string.Equals(changed.OldValue, changed.NewValue))
                {
                    this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_SetIdentity_Success_Set_Format, changed.Key, changed.NewValue)).GetAwaiter().GetResult();
                }
                else
                {
                    this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_SetIdentity_Success_Change_Format, changed.Key, changed.OldValue, changed.NewValue)).GetAwaiter().GetResult();
                }
            };

            await actionExecutor.Execute(action, CancellationToken.None).ConfigureAwait(false);
            return StandardExitCodes.ErrorSuccess;
        }

        protected override async Task<int> Validate()
        {
            var baseResult = await base.Validate().ConfigureAwait(false);
            if (baseResult != StandardExitCodes.ErrorSuccess)
            {
                return baseResult;
            }

            if (this.Verb.Name == null && this.Verb.ProcessorArchitecture == null && this.Verb.Publisher == null && this.Verb.Version == null && this.Verb.ResourceId == null)
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_SetIdentity_Error_NoProperty).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Publisher != null)
            {
                var error = AppxValidatorFactory.ValidateSubject()(this.Verb.Publisher);
                if (error != null)
                {
                    if (this.Verb.Publisher.Contains('='))
                    {
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_SetIdentity_Error_PublisherFormat + " " + error).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_SetIdentity_Error_PublisherFormat + ". " + error + "\r\n" + string.Format(Resources.Localization.CLI_Executor_DidYouMean_Format, "CN=" + this.Verb.Publisher)).ConfigureAwait(false);
                    }

                    return StandardExitCodes.ErrorFormat;
                }
            }

            if (this.Verb.Name != null)
            {
                var error = AppxValidatorFactory.ValidatePackageName()(this.Verb.Name);
                if (error != null)
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_SetIdentity_Error_PackageName + " " + error).ConfigureAwait(false);
                    return StandardExitCodes.ErrorFormat;
                }
            }

            if (this.Verb.ResourceId != null)
            {
                var error = AppxValidatorFactory.ValidateResourceId()(this.Verb.ResourceId);
                if (error != null)
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_SetIdentity_Error_ResourceId + " " + error).ConfigureAwait(false);
                    return StandardExitCodes.ErrorFormat;
                }
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}
