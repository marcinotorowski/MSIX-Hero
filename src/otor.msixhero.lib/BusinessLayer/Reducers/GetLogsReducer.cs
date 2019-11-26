using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.BusinessLayer.Models.Logs;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class GetLogsReducer : SelfElevationReducer<ApplicationState, List<Log>>
    {
        private readonly GetLogs command;

        public GetLogsReducer(GetLogs command, IApplicationStateManager<ApplicationState> stateManager) : base(command, stateManager)
        {
            this.command = command;
        }

        public override async Task<List<Log>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return new List<Log>(await packageManager.GetLogs(this.command.MaxCount, cancellationToken).ConfigureAwait(false));
        }
    }
}
