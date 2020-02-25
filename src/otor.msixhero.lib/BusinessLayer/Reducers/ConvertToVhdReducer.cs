using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class ConvertToVhdReducer : SelfElevationReducer
    {
        private readonly ConvertToVhd command;
        private readonly IAppAttach appAttach;

        public ConvertToVhdReducer(ConvertToVhd command, IElevatedClient elevatedClient, IAppAttach appAttach, IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.appAttach = appAttach;
        }

        protected override Task ReduceAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.appAttach.CreateVolume(
                this.command.PackagePath, 
                this.command.VhdPath, 
                this.command.SizeInMegaBytes, 
                this.command.ExtractCertificate,
                this.command.GenerateScripts, 
                cancellationToken, 
                progress);
        }
    }
}
