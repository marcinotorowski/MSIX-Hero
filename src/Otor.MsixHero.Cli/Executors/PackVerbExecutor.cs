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
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Cli.Executors
{
    public class PackVerbExecutor : VerbExecutor<PackVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackVerbExecutor));

        public PackVerbExecutor(PackVerb verb, IConsole console) : base(verb, console)
        {
        }

        public override async Task<int> Execute()
        {
            var msixSdkWrapper = new MakeAppxWrapper();

            Logger.Info($"Packing [{this.Verb.Directory}] to [{this.Verb.Package}]...");

            try
            {
                await this.Console.WriteInfo($"Packing [{this.Verb.Directory}] to [{this.Verb.Package}]...").ConfigureAwait(false);
                await msixSdkWrapper.PackPackageDirectory(this.Verb.Directory, this.Verb.Package, !this.Verb.NoCompression, !this.Verb.NoValidation).ConfigureAwait(false);

                await this.Console.WriteSuccess($"Package [{this.Verb.Package}] has been created.");

                if (this.Verb.RemoveDirectoryAfterPacking)
                {
                    await this.Console.WriteInfo($"Removing source directory {this.Verb.Directory}...");
                    ExceptionGuard.Guard(() => Directory.Delete(this.Verb.Directory, true));
                }

                return 0;
            }
            catch (SdkException e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message);
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message);
                return 1;
            }
        }
    }
}
