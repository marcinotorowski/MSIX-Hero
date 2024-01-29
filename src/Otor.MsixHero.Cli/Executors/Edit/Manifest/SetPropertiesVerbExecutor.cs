// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Cli.Verbs.Edit.Manifest;

namespace Otor.MsixHero.Cli.Executors.Edit.Manifest
{
    public class SetPropertiesVerbExecutor : ManifestEditVerbExecutor<SetPropertiesEditVerb>
    {
        public SetPropertiesVerbExecutor(string package, SetPropertiesEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task<int> ExecuteOnManifest(XDocument document)
        {
            var action = new SetPackageProperties
            {
                Logo = this.Verb.Logo,
                Description = this.Verb.Description,
                DisplayName = this.Verb.DisplayName,
                ModificationPackage = this.Verb.ModificationPackage,
                PublisherDisplayName = this.Verb.PublisherDisplayName
            };

            var actionExecutor = new SetPackagePropertiesExecutor(document);
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
    }
}