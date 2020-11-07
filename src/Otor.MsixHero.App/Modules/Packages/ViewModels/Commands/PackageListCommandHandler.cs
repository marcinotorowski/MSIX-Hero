using System.Threading;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.Commands
{
    public class PackageListCommandHandler
    {
        private readonly IMsixHeroApplication msixHeroApp;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;

        public PackageListCommandHandler(
            IMsixHeroApplication msixHeroApp, 
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.msixHeroApp = msixHeroApp;
            this.busyManager = busyManager;
            this.interactionService = interactionService;

            this.Refresh = new DelegateCommand(this.OnRefresh, this.OnCanRefresh);
        }

        public ICommand Refresh { get; }

        private async void OnRefresh()
        {
            var executor = this.msixHeroApp.CommandExecutor
                .WithErrorHandling(this.interactionService, true)
                .WithBusyManager(this.busyManager, OperationType.PackageLoading);

            await executor.Invoke(this, new GetPackagesCommand(), CancellationToken.None).ConfigureAwait(false);
        }

        private bool OnCanRefresh()
        {
            return true;
        }
    }
}
