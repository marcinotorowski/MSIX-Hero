using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Elevation;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetLogsHandler : IRequestHandler<GetLogsCommand, IList<Log>>
    {
        private readonly IUacElevation _uacElevation;
        private readonly IBusyManager _busyManager;

        public GetLogsHandler(IUacElevation uacElevation, IBusyManager busyManager)
        {
            this._uacElevation = uacElevation;
            this._busyManager = busyManager;
        }

        public async Task<IList<Log>> Handle(GetLogsCommand request, CancellationToken cancellationToken)
        {
            var context = this._busyManager.Begin(OperationType.EventsLoading);
            try
            {
                return await this._uacElevation.AsHighestAvailable<IAppxLogManager>().GetLogs(request.Count, cancellationToken, context).ConfigureAwait(false);
            }
            finally
            {
                this._busyManager.End(context);
            }
        }
    }
}