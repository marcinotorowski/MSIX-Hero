using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class GetLogsHandler : IRequestHandler<GetLogsCommand, IList<Log>>
    {
        private readonly ISelfElevationProxyProvider<IAppxLogManager> logManagerProvider;
        private readonly IBusyManager busyManager;

        public GetLogsHandler(ISelfElevationProxyProvider<IAppxLogManager> logManagerProvider, IBusyManager busyManager)
        {
            this.logManagerProvider = logManagerProvider;
            this.busyManager = busyManager;
        }

        public async Task<IList<Log>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
        {
            var context = this.busyManager.Begin(OperationType.EventsLoading);
            try
            {
                var manager = await this.logManagerProvider.GetProxyFor(SelfElevationLevel.HighestAvailable, cancellationToken).ConfigureAwait(false);
                return await manager.GetLogs(request.Count, cancellationToken, context).ConfigureAwait(false);
            }
            finally
            {
                this.busyManager.End(context);
            }
        }
    }
}