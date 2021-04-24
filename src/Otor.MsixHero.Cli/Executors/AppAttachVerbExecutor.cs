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

namespace Otor.MsixHero.Cli.Executors
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
                await this.Console.WriteError("This command can be started only by a local administrator.");
                return 11;
            }

            if (this.Verb.Name != null && this.Verb.Package.Count() > 1)
            {
                await this.Console.WriteError("Parameter --name/-n cannot be used with more than one instance of the --package/-p switch.");
                return 2;
            }

            if (this.Verb.Size > 0 && this.Verb.Package.Count() > 1)
            {
                await this.Console.WriteError("Parameter --size/-s cannot be used with more than one instance of the --package/-p switch.");
                return 2;
            }

            if (this.Verb.Size > 0 && this.Verb.FileType == AppAttachVolumeType.Cim)
            {
                await this.Console.WriteError("Parameter --size/-s cannot be used with target type CIM.");
                return 2;
            }

            await this.Console.WriteInfo($"Creating volume(s) in {this.Verb.Directory}...");
            
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
                return e.ExitCode != 0 ? e.ExitCode : 1;
            }
            catch (SdkException e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return e.ExitCode != 0 ? e.ExitCode : 1;
            }
            catch (Win32Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return e.HResult;
            }
            catch (Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return 1;
            }
            
            foreach (var package in this.Verb.Package)
            {
                var createdContainerName = (!string.IsNullOrEmpty(this.Verb.Name) ? this.Verb.Name : Path.GetFileNameWithoutExtension(package)) + "." + this.Verb.FileType.ToString("G").ToLowerInvariant();
                await this.Console.WriteSuccess(" --> Created volume " + createdContainerName);
            }

            if (this.Verb.ExtractCertificate)
            {
                foreach (var package in this.Verb.Package)
                {
                    var createdCertificate = Path.Combine(this.Verb.Directory, Path.GetFileNameWithoutExtension(package)) + ".cer";
                    if (File.Exists(createdCertificate))
                    {
                        await this.Console.WriteSuccess(" --> Extracted certificate file " + Path.GetFileName(createdCertificate));
                    }
                }
            }
            
            if (this.Verb.FileType != AppAttachVolumeType.Cim)
            {
                await this.Console.WriteSuccess(" --> Created definition: app-attach.json with the following parameters:");

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
                    await this.Console.WriteSuccess(" --> Created script: stage.ps1, register.ps1, deregister.ps1, destage.ps1");
                }
            }
            
            return 0;
        }
    }
}
