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
using System.Collections.Generic;
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
        if (!string.IsNullOrEmpty(this.Verb.Output))
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
                await this.Console.WriteError(Resources.Localization.CLI_Executor_SharedContainer_WrongSet);
                return StandardExitCodes.ErrorParameter;
            }

            var builder = await GetBuilder(this.Verb.Name, this.Verb.Packages, this.Console).ConfigureAwait(false);
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

            await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_SharedContainer_SavedInFormat, this.Verb.Output));
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
            await this.Console.WriteError(Resources.Localization.CLI_Executor_SharedContainer_NotSupported);
            return StandardExitCodes.ErrorNotSupported;
        }

        if (!await UserHelper.IsAdministratorAsync(CancellationToken.None).ConfigureAwait(false))
        {
            await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_NotAdmin);
            return StandardExitCodes.ErrorMustBeAdministrator;
        }

        try
        {
            var builder = await GetBuilder(this.Verb.Name, this.Verb.Packages, this.Console).ConfigureAwait(false);

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
                
            var added = await this._sharedPackageContainerService.Add(builder.Build(), this.Verb.ForceApplicationShutdown, resolution, CancellationToken.None).ConfigureAwait(false);

            await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_SharedContainer_DeployedAsFormat, this.Verb.Name)).ConfigureAwait(false);
            await this.Console.WriteSuccess(string.Format(" -> : {0} {2}", Resources.Localization.CLI_Executor_SharedContainer_Id, added.Id)).ConfigureAwait(false);
            await this.Console.WriteSuccess(" -> " + Resources.Localization.CLI_Executor_SharedContainer_Families + ":").ConfigureAwait(false);

            foreach (var pf in added.PackageFamilies)
            {
                await this.Console.WriteSuccess("    * " + pf.FamilyName).ConfigureAwait(false);
            }

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

    private static async Task<SharedPackageContainerBuilder> GetBuilder(string containerName, IEnumerable<string> packages, IConsole console)
    {
        var builder = new SharedPackageContainerBuilder(containerName);
        foreach (var pkg in packages.Where(p => p != null))
        {
            if (File.Exists(pkg))
            {
                await console.WriteInfo(" -> " + string.Format(Resources.Localization.CLI_Executor_SharedContainer_AddingPathFormat, pkg)).ConfigureAwait(false);
                await builder.AddFromFilePath(pkg, CancellationToken.None).ConfigureAwait(false);
            }
            else if (Regex.IsMatch(pkg, "^[A-Za-z0-9][-\\.A-Za-z0-9]+_[A-Za-z0-9]{13}$"))
            {
                await console.WriteInfo(" -> " + string.Format(Resources.Localization.CLI_Executor_SharedContainer_AddingFamilyFormat, pkg)).ConfigureAwait(false);
                builder.AddFamilyName(pkg);
            }
            else
            {
                await console.WriteInfo(" -> " + string.Format(Resources.Localization.CLI_Executor_SharedContainer_AddingFullNameFormat, pkg)).ConfigureAwait(false);
                builder.AddFullPackageName(pkg);
            }
        }

        return builder;
    }
}