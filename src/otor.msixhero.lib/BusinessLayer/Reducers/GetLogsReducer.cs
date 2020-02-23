using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class GetLogsReducer : SelfElevationReducer<List<Log>>
    {
        private readonly GetLogs command;

        public GetLogsReducer(GetLogs command, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
        }

        public override async Task<List<Log>> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return new List<Log>(await packageManager.GetLogs(this.command.MaxCount, cancellationToken).ConfigureAwait(false));
        }
    }
}
