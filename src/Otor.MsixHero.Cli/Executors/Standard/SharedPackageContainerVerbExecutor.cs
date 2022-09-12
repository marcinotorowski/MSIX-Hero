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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Builder;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Cli.Executors.Standard;

public class SharedPackageContainerVerbExecutor : VerbExecutor<SharedPackageContainerVerb>
{
    private readonly ISharedPackageContainerService _sharedPackageContainerService;
    private static readonly LogSource Logger = new();

    public SharedPackageContainerVerbExecutor(SharedPackageContainerVerb verb, ISharedPackageContainerService sharedPackageContainerService, IConsole console) : base(verb, console)
    {
        _sharedPackageContainerService = sharedPackageContainerService;
    }

    public override Task<int> Execute()
    {
        if (this.Verb.Output == null)
        {
            return this.ExecuteXml();
        }

        return this.ExecuteDeploy();
    }

    private async Task<int> ExecuteXml()
    {
        try
        {
            if (this.Verb.Force || this.Verb.ForceApplicationShutdown || this.Verb.Merge)
            {
                await this.Console.WriteError("Parameter --output cannot be used if any of the following parameters are used: --force, -f, --forceApplicationShutdown, --merge, -m.");
                return StandardExitCodes.ErrorParameter;
            }

            var builder = new SharedPackageContainerBuilder(this.Verb.Name);
            foreach (var pkg in this.Verb.Packages.Where(p => p != null))
            {
                if (File.Exists(pkg))
                {
                    await this.Console.WriteInfo(string.Format("Adding file path {0}...", pkg)).ConfigureAwait(false);
                    await builder.AddFromFilePath(pkg, CancellationToken.None).ConfigureAwait(false);
                }
                else if (Regex.IsMatch(pkg, "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$"))
                {
                    await this.Console.WriteInfo(string.Format("Adding family name {0}...", pkg)).ConfigureAwait(false);
                    builder.AddFamilyName(pkg);
                }
                else
                {
                    await this.Console.WriteInfo(string.Format("Adding full package name {0}...", pkg)).ConfigureAwait(false);
                    builder.AddFullPackageName(pkg);
                }
            }

            var xml = builder.ToXml();

            var output = new FileInfo(this.Verb.Output);
            if (output.Exists)
            {
                output.Delete();
            }
            else if (output.Directory?.Exists == false)
            {
                output.Directory.Create();
            }

            await File.WriteAllTextAsync(output.FullName, xml).ConfigureAwait(false);
            return StandardExitCodes.ErrorSuccess;
        }
        catch (SdkException e)
        {
            Logger.Error().WriteLine(e);
            await this.Console.WriteError(e.Message);
            return e.ExitCode;
        }
        catch (Exception e)
        {
            Logger.Error().WriteLine(e);
            await this.Console.WriteError(e.Message);
            return StandardExitCodes.ErrorGeneric;
        }
    }
        
    private async Task<int> ExecuteDeploy()
    {
        if (!this._sharedPackageContainerService.IsSharedPackageContainerSupported())
        {
            await this.Console.WriteError("This operation is not supported on this version of Windows. You need at least Windows 11 build 21354 (10.0.21354) to use this feature.");
            return StandardExitCodes.ErrorNotSupported;
        }

        if (!await UserHelper.IsAdministratorAsync(CancellationToken.None).ConfigureAwait(false))
        {
            await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_NotAdmin);
            return StandardExitCodes.ErrorMustBeAdministrator;
        }

        try
        {
            var builder = new SharedPackageContainerBuilder(this.Verb.Name);
            foreach (var pkg in this.Verb.Packages.Where(p => p != null))
            {
                if (File.Exists(pkg))
                {
                    await this.Console.WriteInfo(string.Format("Adding file path {0}...", pkg)).ConfigureAwait(false);
                    await builder.AddFromFilePath(pkg, CancellationToken.None).ConfigureAwait(false);
                }
                else if (Regex.IsMatch(pkg, "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$"))
                {
                    await this.Console.WriteInfo(string.Format("Adding family name {0}...", pkg)).ConfigureAwait(false);
                    builder.AddFamilyName(pkg);
                }
                else
                {
                    await this.Console.WriteInfo(string.Format("Adding full package name {0}...", pkg)).ConfigureAwait(false);
                    builder.AddFullPackageName(pkg);
                }
            }

            ContainerConflictResolution resolution;
            if (this.Verb.Force)
            {
                resolution = ContainerConflictResolution.Replace;
            }
            else if (this.Verb.Merge)
            {
                resolution = ContainerConflictResolution.Merge;
            }
            else
            {
                resolution = ContainerConflictResolution.Default;
            }
                
            await this._sharedPackageContainerService.Add(builder.Build(), this.Verb.ForceApplicationShutdown, resolution, CancellationToken.None).ConfigureAwait(false);

            return StandardExitCodes.ErrorSuccess;
        }
        catch (SdkException e)
        {
            Logger.Error().WriteLine(e);
            await this.Console.WriteError(e.Message);
            return e.ExitCode;
        }
        catch (Exception e)
        {
            Logger.Error().WriteLine(e);
            await this.Console.WriteError(e.Message);
            return StandardExitCodes.ErrorGeneric;
        }
    }
}