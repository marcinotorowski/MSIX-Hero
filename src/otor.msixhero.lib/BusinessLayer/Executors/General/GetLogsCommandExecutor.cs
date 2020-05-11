using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Executors.General
{
    public class GetLogsCommandExecutor : CommandWithOutputExecutor<List<Log>>
    {
        private readonly GetLogs command;
        private readonly ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory;

        public GetLogsCommandExecutor(GetLogs command, ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
            this.command = command;
            this.packageManagerFactory = packageManagerFactory;
        }

        public override async Task<List<Log>> ExecuteAndReturn(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            var manager = await this.packageManagerFactory.Get(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            return await manager.GetLogs(this.command.MaxCount, cancellationToken, progressData).ConfigureAwait(false);
        }
    }
}
