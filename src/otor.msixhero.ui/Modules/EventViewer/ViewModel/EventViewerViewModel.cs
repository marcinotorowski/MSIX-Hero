using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.ui.Controls.Progress;
using otor.msixhero.ui.Modules.Common;
using otor.msixhero.ui.Themes;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.EventViewer.ViewModel
{
    public class EventViewerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private bool isActive;
        private readonly IApplicationStateManager stateManager;
        private readonly IInteractionService interactionService;
        private bool firstRun = true;

        public EventViewerViewModel(IApplicationStateManager stateManager, IInteractionService interactionService)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            this.Logs = new ObservableCollection<Log>();
            this.LogsView = CollectionViewSource.GetDefaultView(this.Logs);
            this.Sort(nameof(Log.DateTime), false);
            this.CommandHandler = new EventViewerCommandHandler(this, stateManager);
        }

        public EventViewerCommandHandler CommandHandler { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public async Task<ObservableCollection<Log>> GetLogs()
        {
            try
            {
                this.Progress.IsLoading = true;

                var action = new GetLogs(250);
                var result = await this.stateManager.CommandExecutor.GetExecuteAsync(action).ConfigureAwait(false);
                return new ObservableCollection<Log>(result);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError("Could not get last 250 logs.", e);
                return new ObservableCollection<Log>();
            }
            finally
            {
                this.Progress.IsLoading = false;
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
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.IsActiveChanged?.Invoke(this, new EventArgs());

                if (value)
                {
                    try
                    {
                        this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.EventViewer));
                    }
                    catch (UserHandledException)
                    {
                        return;
                    }

                    if (this.firstRun)
                    {
                        this.firstRun = false;
                        this.Reload();
                    }
                }
            }
        }

        public void Reload()
        {
            this.Progress.Progress = -1;
            this.Progress.Message = "Loading recent events...";
            this.Progress.IsLoading = true;

            this.GetLogs().ContinueWith(
                t =>
                {
                    this.Progress.Progress = 100;
                    this.Progress.IsLoading = false;
                    this.Logs.Clear();

                    foreach (var item in t.Result)
                    {
                        this.Logs.Add(item);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            try
            {
                this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.EventViewer));
            }
            catch (UserHandledException)
            {
            }
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
    }
}

