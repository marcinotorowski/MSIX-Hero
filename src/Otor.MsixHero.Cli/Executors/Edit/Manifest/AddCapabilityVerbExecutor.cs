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
    public class AddCapabilityVerbExecutor : ManifestEditVerbExecutor<AddCapabilityVerb>
    {
        public AddCapabilityVerbExecutor(string package, AddCapabilityVerb verb, IConsole console) : base(package, verb, console)
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
                await this.Console.WriteError("Capability name cannot be empty.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Name.Length > 50)
            {
                await this.Console.WriteError($"The value '{this.Verb.Name}' is longer than 50 characters.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Name.IndexOf('{') != -1 && !Guid.TryParse(this.Verb.Name, out var _))
            {
                await this.Console.WriteError($"The value '{this.Verb.Name}' is not a GUID nor a valid string.").ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }

            return StandardExitCodes.ErrorSuccess;
        }
        
        protected override async Task<int> ExecuteOnManifest(XDocument document)
        {
            var action = new AddCapability
            {
                Name = this.Verb.Name
            };

            var executor = new AddCapabilityExecutor(document);
            executor.CapabilityAdded += (_, changed) =>
            {
                if (changed.IsCustom)
                {
                    this.Console.WriteSuccess($"Added custom capability '{changed.Name}'.").GetAwaiter().GetResult();
                }
                else if (changed.IsRestricted)
                {
                    this.Console.WriteSuccess($"Added restricted capability '{changed.Name}'.").GetAwaiter().GetResult();
                }
                else
                {
                    this.Console.WriteSuccess($"Added standard capability '{changed.Name}'.").GetAwaiter().GetResult();
                }
            };

            try
            {
                await executor.Execute(action).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await this.Console.WriteError($"Capability '{this.Verb.Name}' could not be added.").ConfigureAwait(false);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            return StandardExitCodes.ErrorSuccess;
        }
    }
}