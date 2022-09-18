using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.EventViewer;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Diagnostic.Events;
using Otor.MsixHero.Appx.Diagnostic.Events.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class GetEventsHandler : IRequestHandler<GetEventsCommand, IList<AppxEvent>>
    {
        private readonly IConfigurationService _configurationService;
        private readonly IMsixHeroApplication _application;
        private readonly IUacElevation _uacElevation;
        private readonly IBusyManager _busyManager;

        public GetEventsHandler(
            IConfigurationService configurationService,
            IMsixHeroApplication application,
            IUacElevation uacElevation, 
            IBusyManager busyManager)
        {
            this._configurationService = configurationService;
            this._application = application;
            this._uacElevation = uacElevation;
            this._busyManager = busyManager;
        }

        public async Task<IList<AppxEvent>> Handle(GetEventsCommand request, CancellationToken cancellationToken)
        {
            var context = this._busyManager.Begin(OperationType.EventsLoading);
            try
            {
                var logs = await this._uacElevation.AsHighestAvailable<IAppxEventService>().GetEvents(request.Criteria, cancellationToken, context).ConfigureAwait(false);
                this._application.ApplicationState.EventViewer.Criteria = request.Criteria;


                var cleanConfig = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
                cleanConfig.Events ??= new EventsConfiguration();
                cleanConfig.Events.Filter ??= new EventsFilterConfiguration();
                if (request.Criteria?.TimeSpan != default)
                {
                    cleanConfig.Events.TimeSpan = request.Criteria.TimeSpan.Value;
                }
                else
                {
                    cleanConfig.Events.TimeSpan = default;
                }

                await this._configurationService.SetCurrentConfigurationAsync(cleanConfig, cancellationToken).ConfigureAwait(false);
                return logs;
            }
            finally
            {
                this._busyManager.End(context);
            }
        }
    }
}