using System.Windows.Input;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands.Logs;
using Otor.MsixHero.Ui.Hero.Executor;

namespace Otor.MsixHero.Ui.Modules.EventViewer.ViewModel
{
    public class EventViewerCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;
        private ICommand refresh, openLogs;

        public EventViewerCommandHandler(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
        }

        public ICommand Refresh
        {
            get
            {
                return this.refresh ??= new DelegateCommand(this.RefreshExecute);
            }
        }

        public ICommand OpenLogs
        {
            get
            {
                return this.openLogs ??= new DelegateCommand(this.OpenLogsExecute);
            }
        }

        private void OpenLogsExecute(object obj)
        {
            this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .Invoke(this, new OpenEventViewerCommand(obj is EventLogCategory elt ? elt : EventLogCategory.AppXDeploymentOperational));
        }

        public void RefreshExecute(object param)
        {
            this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.EventsLoading)
                .WithErrorHandling(this.interactionService, true)
                .Invoke(this, new GetLogsCommand()).ConfigureAwait(false);
        }
    }
}
