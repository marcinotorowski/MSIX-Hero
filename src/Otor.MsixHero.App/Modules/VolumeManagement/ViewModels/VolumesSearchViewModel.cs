using System.Threading;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;

        public VolumesSearchViewModel(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetVolumeFilterCommand>>().Subscribe(this.OnSetVolumeFilterCommand);
        }

        private void OnSetVolumeFilterCommand(UiExecutedPayload<SetVolumeFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }


        public string SearchKey
        {
            get => this.application.ApplicationState.Volumes.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetVolumeFilterCommand(value));
        }
        
        private async void LoadContext()
        {
            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.VolumeLoading)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke(this, new GetVolumesCommand(), CancellationToken.None).ConfigureAwait(false);
        }
    }
}
