using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public class GetLogsReducer : SelfElevationReducer<List<Log>>
    {
        private readonly GetLogs command;
        private readonly IAppxPackageManager packageManager;

        public GetLogsReducer(GetLogs command, IElevatedClient elevatedClient, IAppxPackageManager packageManager, IWritableApplicationStateManager stateManager) : base(command, elevatedClient, stateManager)
        {
            this.command = command;
            this.packageManager = packageManager;
        }

        protected override async Task<List<Log>> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = default)
        {
            return new List<Log>(await this.packageManager.GetLogs(this.command.MaxCount, cancellationToken).ConfigureAwait(false));
        }
    }
}
