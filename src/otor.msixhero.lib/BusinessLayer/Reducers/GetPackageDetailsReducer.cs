using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackageDetailsReducer : BaseReducer<ApplicationState, AppxPackage>
    {
        private readonly GetPackageDetails command;

        public GetPackageDetailsReducer(GetPackageDetails command, IApplicationStateManager<ApplicationState> state) : base(command, state)
        {
            this.command = command;
        }

        public override async Task<AppxPackage> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            var details = await packageManager.Get(this.command.PackageFullName, cancellationToken).ConfigureAwait(false);
            return details;
        }
    }
}
