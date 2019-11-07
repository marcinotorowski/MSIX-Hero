using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetPackageDetailsReducer : BaseReducer<ApplicationState, AppxPackage>
    {
        private readonly GetPackageDetails command;
        private readonly IAppxPackageManager packageManager;

        public GetPackageDetailsReducer(GetPackageDetails command, IApplicationStateManager<ApplicationState> state, IAppxPackageManager packageManager) : base(command, state)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        public override async Task<AppxPackage> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default)
        {
            var details = await this.packageManager.Get(this.command.PackageFullName, cancellationToken).ConfigureAwait(false);
            return details;
        }
    }
}
