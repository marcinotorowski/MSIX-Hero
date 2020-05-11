using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    internal class ConvertToVhdCommandExecutor : CommandExecutor
    {
        private readonly ConvertToVhd command;
        private readonly ISelfElevationManagerFactory<IAppAttachManager> appAttachManagerFactory;

        public ConvertToVhdCommandExecutor(
            ConvertToVhd command, 
            ISelfElevationManagerFactory<IAppAttachManager> appAttachManagerFactory, 
            IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.appAttachManagerFactory = appAttachManagerFactory;
        }

        public override async Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.appAttachManagerFactory.Get(SelfElevationLevel.AsAdministrator, cancellationToken).ConfigureAwait(false);
            
            cancellationToken.ThrowIfCancellationRequested();

            await manager.CreateVolume(
                this.command.PackagePath,
                this.command.VhdPath,
                this.command.SizeInMegaBytes,
                this.command.ExtractCertificate,
                this.command.GenerateScripts,
                cancellationToken,
                progressData).ConfigureAwait(false); ;
        }
    }
}
