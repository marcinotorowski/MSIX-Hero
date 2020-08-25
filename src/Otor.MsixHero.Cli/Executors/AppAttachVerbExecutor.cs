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
            if (!(await UserHelper.IsAdministratorAsync(CancellationToken.None).ConfigureAwait(false)))
            {
                await this.Console.WriteError("This command can be started only by a local administrator.");
                return 11;
            }

            if (!File.Exists(this.Verb.Package))
            {
                await this.Console.WriteError($"The file {this.Verb.Package} does not exist.").ConfigureAwait(false);
                return 10;
            }

            if (!Directory.Exists(this.Verb.Directory))
            {
                Directory.CreateDirectory(this.Verb.Directory);
            }

            var volumeName = string.IsNullOrEmpty(this.Verb.Name) ? Path.GetFileNameWithoutExtension(this.Verb.Package) : this.Verb.Name;
            var volumePath = Path.Combine(this.Verb.Directory, volumeName + ".vhd");

            try
            {
                await this.Console.WriteInfo($"Creating VHD volume in {this.Verb.Directory}...");
                await this.appAttachManager.CreateVolume(this.Verb.Package, volumePath, this.Verb.Size, this.Verb.ExtractCertificate, this.Verb.CreateScript).ConfigureAwait(false);

                await this.Console.WriteSuccess($"The volume has been created in {volumePath}");

                await this.Console.WriteSuccess($" --> Created definition: app-attach.json with the following parameters:");

                var json = await File.ReadAllTextAsync(Path.Combine(this.Verb.Directory, "app-attach.json")).ConfigureAwait(false);
                var jsonArray = (JObject)(((JArray)JToken.Parse(json))[0]);

                var maxPropNameLength = jsonArray.Properties().Select(p => p.Name.Length).Max() + " --> ".Length;
                foreach (var prop in jsonArray)
                {
                    await this.Console.WriteSuccess("".PadLeft(" --> ".Length, ' ') + prop.Key.PadRight(maxPropNameLength, ' ') + " = " + prop.Value.Value<string>());
                }

                if (this.Verb.CreateScript)
                {
                    await this.Console.WriteSuccess($" --> Created script: stage.ps1, register.ps1, deregister.ps1, destage.ps1");
                }

                if (this.Verb.ExtractCertificate)
                {
                    await this.Console.WriteSuccess($" --> Extracted certificate: {volumeName}.cer");
                }

                return 0;
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
        }
    }
}
