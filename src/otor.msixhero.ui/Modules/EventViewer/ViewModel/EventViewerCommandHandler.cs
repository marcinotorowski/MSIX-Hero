using System;
using System.Windows.Input;
using Otor.MsixHero.Lib.BusinessLayer.State;
using Otor.MsixHero.Lib.Domain.Commands.EventViewer;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Hero;

namespace Otor.MsixHero.Ui.Modules.EventViewer.ViewModel
{
    public class EventViewerCommandHandler
    {
        private readonly EventViewerViewModel viewModel;
        private readonly IMsixHeroApplication application;
        private ICommand refresh, openLogs;

        public EventViewerCommandHandler(EventViewerViewModel viewModel, IMsixHeroApplication application)
        {
            this.viewModel = viewModel;
            this.application = application;
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
            // todo:
            throw new NotImplementedException();
            // this.stateManager.CommandExecutor.ExecuteAsync(new OpenEventViewer(obj is EventLogType elt ? elt : EventLogType.AppXDeploymentOperational));
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
