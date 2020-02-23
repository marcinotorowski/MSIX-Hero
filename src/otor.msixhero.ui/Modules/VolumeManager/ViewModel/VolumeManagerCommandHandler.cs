using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.ui.Commands.RoutedCommand;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerCommandHandler
    {
        private readonly IApplicationStateManager stateManager;

        public VolumeManagerCommandHandler(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.Refresh = new DelegateCommand(this.RefreshExecute);
        }

        public ICommand Refresh { get; }

        private void RefreshExecute(object obj)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new GetVolumes());
        }
    }
}
