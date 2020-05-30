using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.EventViewer;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Commands.RoutedCommand;

namespace otor.msixhero.ui.Modules.EventViewer.ViewModel
{
    public class EventViewerCommandHandler
    {
        private readonly EventViewerViewModel viewModel;
        private readonly IApplicationStateManager stateManager;
        private ICommand refresh, openLogs;

        public EventViewerCommandHandler(EventViewerViewModel viewModel, IApplicationStateManager stateManager)
        {
            this.viewModel = viewModel;
            this.stateManager = stateManager;
        }

        public ICommand Refresh
        {
            get
            {
                return this.refresh ??= new DelegateCommand(this.RefreshExecute, this.CanExecuteRefresh);
            }
        }

        public ICommand OpenLogs
        {
            get
            {
                return this.openLogs ??= new DelegateCommand(this.OpenLogsExecute, this.CanExecuteOpenLogs);
            }
        }

        private void OpenLogsExecute(object obj)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new OpenEventViewer(obj is EventLogType elt ? elt : EventLogType.AppXDeploymentOperational));
        }

        private bool CanExecuteOpenLogs(object obj)
        {
            return true;
        }

        private bool CanExecuteRefresh(object obj)
        {
            return !this.viewModel.Progress.IsLoading;
        }

        public void RefreshExecute(object param)
        {
            this.viewModel.Reload();
        }
    }
}
