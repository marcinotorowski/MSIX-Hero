using System.Threading;
using System.Windows;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Logs;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.EventViewer.ViewModels
{
    public class EventViewerCommandHandler
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;

        public EventViewerCommandHandler(
            UIElement parent,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.busyManager = busyManager;

            parent.CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, this.OnRefresh, this.CanRefresh));
        }

        private async void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            var executor = this.application.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.EventsLoading);

            await executor.Invoke(this, new GetLogsCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private void CanRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
