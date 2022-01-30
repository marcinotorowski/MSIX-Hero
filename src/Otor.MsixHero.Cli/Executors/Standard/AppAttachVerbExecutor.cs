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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class AppAttachVerbExecutor : VerbExecutor<AppAttachVerb>
    {
        private readonly IAppAttachManager appAttachManager;

        public AppAttachVerbExecutor(AppAttachVerb verb, IAppAttachManager appAttachManager, IConsole console) : base(verb, console)
        {
            this.appAttachManager = appAttachManager;
        }

        public override async Task<int> Execute()
        {
            if (!await UserHelper.IsAdministratorAsync(CancellationToken.None).ConfigureAwait(false))
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_NotAdmin);
                return StandardExitCodes.ErrorMustBeAdministrator;
            }

            if (this.Verb.Name != null && this.Verb.Package.Count() > 1)
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_NameSwitchConflict);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Size > 0 && this.Verb.Package.Count() > 1)
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_SizeSwitchConflict);
                return StandardExitCodes.ErrorParameter;
            }

            if (this.Verb.Size > 0 && this.Verb.FileType == AppAttachVolumeType.Cim)
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_AppAttach_Error_SizeCimSwitchConflict);
                return StandardExitCodes.ErrorParameter;
            }

            await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_AppAttach_CreatingVolumes_Format, this.Verb.Directory));
            
            try
            {
                if (this.Verb.Package.Count() == 1)
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    var volumeName = Path.Combine(this.Verb.Directory, this.Verb.Name ?? Path.GetFileNameWithoutExtension(this.Verb.Package.First())) + "." + this.Verb.FileType.ToString("G").ToLowerInvariant();
                    await this.appAttachManager.CreateVolume(Verb.Package.First(), volumeName, this.Verb.Size, this.Verb.FileType, this.Verb.ExtractCertificate, this.Verb.CreateScript).ConfigureAwait(false);
                }
                else
                {
                    await this.appAttachManager.CreateVolumes(Verb.Package.ToList(), this.Verb.Directory, this.Verb.FileType, this.Verb.ExtractCertificate, this.Verb.CreateScript).ConfigureAwait(false);
                }
            }
            catch (ProcessWrapperException e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return e.ExitCode != StandardExitCodes.ErrorSuccess ? e.ExitCode : StandardExitCodes.ErrorGeneric;
            }
            catch (SdkException e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return e.ExitCode != StandardExitCodes.ErrorSuccess ? e.ExitCode : StandardExitCodes.ErrorGeneric;
            }
            catch (Win32Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return e.HResult;
            }
            catch (Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }
            
            foreach (var package in this.Verb.Package)
            {
                var createdContainerName = (!string.IsNullOrEmpty(this.Verb.Name) ? this.Verb.Name : Path.GetFileNameWithoutExtension(package)) + "." + this.Verb.FileType.ToString("G").ToLowerInvariant();
                await this.Console.WriteSuccess(" --> " + string.Format(Resources.Localization.CLI_Executor_AppAttach_Success_Format, createdContainerName));
            }

            if (this.Verb.ExtractCertificate)
            {
                foreach (var package in this.Verb.Package)
                {
                    var createdCertificate = Path.Combine(this.Verb.Directory, Path.GetFileNameWithoutExtension(package)) + ".cer";
                    if (File.Exists(createdCertificate))
                    {
                        await this.Console.WriteSuccess(" --> " + string.Format(Resources.Localization.CLI_Executor_AppAttach_Success_Certificate_Format, Path.GetFileName(createdCertificate)));
                    }
                }
            }
            
            if (this.Verb.FileType != AppAttachVolumeType.Cim)
            {
                await this.Console.WriteSuccess(" --> " + string.Format(Resources.Localization.CLI_Executor_AppAttach_Success_Json_Format, "app-attach.json"));

                var json = await File.ReadAllTextAsync(Path.Combine(this.Verb.Directory, "app-attach.json")).ConfigureAwait(false);
                var jsonArray = (JArray)JToken.Parse(json);
                foreach (var jsonObject in jsonArray.OfType<JObject>())
                {
                    var maxPropNameLength = jsonObject.Properties().Select(p => p.Name.Length).Max() + " --> ".Length;
                    foreach (var prop in jsonObject)
                    {
                        await this.Console.WriteSuccess("".PadLeft(" --> ".Length, ' ') + prop.Key.PadRight(maxPropNameLength, ' ') + " = " + prop.Value?.Value<string>());
                    }
                }

                if (this.Verb.CreateScript)
                {
                    await this.Console.WriteSuccess(" --> " + string.Format(Resources.Localization.CLI_Executor_AppAttach_Success_Scripts_Format, "stage.ps1", "register.ps1", "deregister.ps1", "destage.ps1"));
                }
            }
            
            return StandardExitCodes.ErrorSuccess;
        }
    }
}
