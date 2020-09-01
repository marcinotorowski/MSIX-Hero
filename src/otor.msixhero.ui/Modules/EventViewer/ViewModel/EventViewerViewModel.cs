using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Controls.Progress;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.Commands.Logs;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Hero.State;
using Otor.MsixHero.Ui.Modules.Common;
using Otor.MsixHero.Ui.Themes;
using Otor.MsixHero.Ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.EventViewer.ViewModel
{
    public class EventViewerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private bool isActive;
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;
        private bool firstRun = true;
        private string searchKey;

        public EventViewerViewModel(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.Logs = new ObservableCollection<Log>();
            this.LogsView = CollectionViewSource.GetDefaultView(this.Logs);
            this.LogsView.Filter += Filter;
            this.Sort(nameof(Log.DateTime), false);
            this.CommandHandler = new EventViewerCommandHandler(this.application, busyManager, interactionService);
            this.MaxLogs = 250;
            this.End = DateTime.Now;
            this.Start = this.End.Subtract(TimeSpan.FromDays(5));

            this.busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.Progress = new ProgressProperty();
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetLogsCommand, IList<Log>>>().Subscribe(this.OnGetLogs, ThreadOption.UIThread);
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.EventsLoading)
            {
                return;
            }

            this.Progress.Progress = e.Progress;
            this.Progress.Message = e.Message;
            this.Progress.IsLoading = e.IsBusy;
        }

        public ProgressProperty Progress { get; } 

        private void OnGetLogs(UiExecutedPayload<GetLogsCommand, IList<Log>> obj)
        {
            foreach (var item in obj.Result)
            {
                this.Logs.Add(item);
            }
        }

        public EventViewerCommandHandler CommandHandler { get; }

        public string SearchKey
        {
            get => this.searchKey;
            set
            {
                if (!this.SetField(ref this.searchKey, value))
                {
                    return;
                }

                this.LogsView.Refresh();
            }
        }
        
        public ObservableCollection<Log> Logs { get; }

        public ICollectionView LogsView { get; }

        public string Header { get; } = "Events and logs";

        public Geometry Icon { get; } = VectorIcons.TabEventViewer;
        
        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (!this.SetField(ref this.isActive, value))
                {
                    return;
                }

                this.IsActiveChanged?.Invoke(this, new EventArgs());
                this.SetIsActive(value);
            }
        }

        private async void SetIsActive(bool value)
        {
            if (!value)
            {
                return;
            }

            await this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.EventViewer)).ConfigureAwait(true);

            if (!this.firstRun)
            {
                return;
            }

            this.firstRun = false;

            using (var cts = new CancellationTokenSource())
            {
                using (var task = this.application.CommandExecutor
                    .WithBusyManager(this.busyManager, OperationType.EventsLoading)
                    .WithErrorHandling(this.interactionService, true)
                    .Invoke(this, new GetLogsCommand(), cts.Token))
                {
                    this.Progress.MonitorProgress(task, cts);
                    await task.ConfigureAwait(false);
                }
            }
        }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int MaxLogs { get; set; }
        
        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.EventViewer));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public void Sort(string columnName, bool descending)
        {
            this.LogsView.SortDescriptions.Clear();
            this.LogsView.SortDescriptions.Add(new SortDescription(columnName, descending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }

        private bool Filter(object obj)
        {
            if (string.IsNullOrEmpty(this.searchKey))
            {
                return true;
            }

            var filtered = (Log)obj;

            if (filtered.Message != null && filtered.Message.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            if (filtered.Level != null && filtered.Level.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            if (filtered.User != null && filtered.User.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            if (filtered.DateTime != default && filtered.DateTime.ToString(CultureInfo.CurrentUICulture).IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            if (
                filtered.ActivityId.ToString(CultureInfo.CurrentUICulture).IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1 ||
                filtered.ThreadId.ToString(CultureInfo.CurrentUICulture).IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }

            return false;
        }
    }
}

