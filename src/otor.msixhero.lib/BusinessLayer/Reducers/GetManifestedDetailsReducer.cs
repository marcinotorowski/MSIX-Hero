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
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal class GetManifestedDetailsReducer : BaseReducer<ApplicationState, AppxManifestSummary>
    {
        private readonly GetManifestedDetails command;

        public GetManifestedDetailsReducer(GetManifestedDetails command, IApplicationStateManager<ApplicationState> state) : base(command, state)
        {
            this.command = command;
        }

        public override async Task<AppxManifestSummary> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default)
        {
            var packageManifestLocation = this.command.AppxManifestFilePath;
            var manifestDetails = AppxManifestSummaryBuilder.FromManifest(packageManifestLocation, false);
            return await manifestDetails.ConfigureAwait(false);
        }
    }
}
