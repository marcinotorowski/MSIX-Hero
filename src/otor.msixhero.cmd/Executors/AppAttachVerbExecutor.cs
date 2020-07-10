using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.cmd.Verbs;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Interop;

namespace otor.msixhero.cmd.Executors
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

                if (this.Verb.ExtractCertificate)
                {
                    await this.Console.WriteSuccess($" --> Created script: {volumeName}.stage.ps1");
                    await this.Console.WriteSuccess($" --> Created script: {volumeName}.register.ps1");
                    await this.Console.WriteSuccess($" --> Created script: {volumeName}.deregister.ps1");
                    await this.Console.WriteSuccess($" --> Created script: {volumeName}.destage.ps1");
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
