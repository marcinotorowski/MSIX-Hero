using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class ConvertToVhdReducer : SelfElevationReducer
    {
        private readonly ConvertToVhd command;
        private readonly IAppAttach appAttach;
        private readonly IBusyManager busyManager;

        public ConvertToVhdReducer(ConvertToVhd command, IWritableApplicationStateManager stateManager, IAppAttach appAttach, IBusyManager busyManager) : base(command, stateManager)
        {
            this.command = command;
            this.appAttach = appAttach;
            this.busyManager = busyManager;
        }

        public override async Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var context = this.busyManager.Begin();
            try
            {
                await this.appAttach.CreateVolume(
                    this.command.PackagePath, 
                    this.command.VhdPath, 
                    this.command.SizeInMegaBytes, 
                    this.command.ExtractCertificate,
                    this.command.GenerateScripts, 
                    cancellationToken, 
                    context).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}
