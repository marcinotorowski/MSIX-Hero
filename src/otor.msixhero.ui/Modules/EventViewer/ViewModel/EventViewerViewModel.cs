using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Org.BouncyCastle.Bcpg.OpenPgp;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Progress;
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
        private string searchKey;

        public EventViewerViewModel(IApplicationStateManager stateManager, IInteractionService interactionService)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            this.Logs = new ObservableCollection<Log>();
            this.LogsView = CollectionViewSource.GetDefaultView(this.Logs);
            this.LogsView.Filter += Filter;
            this.Sort(nameof(Log.DateTime), false);
            this.CommandHandler = new EventViewerCommandHandler(this, stateManager);
            this.MaxLogs = 250;
            this.End = DateTime.Now;
            this.Start = this.End.Subtract(TimeSpan.FromDays(5));
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

        public ProgressProperty Progress { get; } = new ProgressProperty { SupportsCancelling = true };

        public async Task<IList<Log>> GetLogs()
        {
            try
            {
                this.Progress.IsLoading = true;

                var action = new GetLogs(this.MaxLogs);

                using (var cts = new CancellationTokenSource())
                {
                    var p = new Progress<ProgressData>();
                    var task = this.stateManager.CommandExecutor.GetExecuteAsync(action, cts.Token, p);
                    this.Progress.MonitorProgress(task, cts, p);
                    var result = await task.ConfigureAwait(false);
                    return new List<Log>(result);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (UserHandledException)
            {
                throw;
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

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public int MaxLogs { get; set; }

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

