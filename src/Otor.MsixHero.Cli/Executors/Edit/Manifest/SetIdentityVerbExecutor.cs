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
                    this.Console.WriteSuccess($"Changed identity attribute '{changed.Key}' to '{changed.NewValue}'.").GetAwaiter().GetResult();
                }
                else
                {
                    this.Console.WriteSuccess($"Changed identity attribute '{changed.Key}' from '{changed.OldValue}' to '{changed.NewValue}'.").GetAwaiter().GetResult();
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
                await this.Console.WriteError("At least one property to change is required.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Publisher != null)
            {
                var error = AppxValidatorFactory.ValidateSubject()(this.Verb.Publisher);
                if (error != null)
                {
                    if (this.Verb.Publisher.Contains('='))
                    {
                        await this.Console.WriteError("The format of the publisher is invalid. " + error).ConfigureAwait(false);
                    }
                    else
                    {
                        await this.Console.WriteError($"The format of the publisher is invalid. {error}\r\nDid you mean CN={this.Verb.Publisher}?").ConfigureAwait(false);
                    }

                    return StandardExitCodes.ErrorFormat;
                }
            }

            if (this.Verb.Name != null)
            {
                var error = AppxValidatorFactory.ValidatePackageName()(this.Verb.Name);
                if (error != null)
                {
                    await this.Console.WriteError("The format of the package name is invalid. " + error).ConfigureAwait(false);
                    return StandardExitCodes.ErrorFormat;
                }
            }

            if (this.Verb.ResourceId != null)
            {
                var error = AppxValidatorFactory.ValidateResourceId()(this.Verb.ResourceId);
                if (error != null)
                {
                    await this.Console.WriteError("The format of the resource ID is invalid. " + error).ConfigureAwait(false);
                    return StandardExitCodes.ErrorFormat;
                }
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}
