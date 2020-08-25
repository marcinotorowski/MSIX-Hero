using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.Controls.Progress;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Modules.Common;
using Otor.MsixHero.Ui.Themes;
using Otor.MsixHero.Ui.ViewModel;
using Prism;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.EventViewer.ViewModel
{
    public class EventViewerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private bool isActive;
        private readonly IMsixHeroApplication application;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly IInteractionService interactionService;
        private bool firstRun = true;
        private string searchKey;

        public EventViewerViewModel(
            IMsixHeroApplication application,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            IInteractionService interactionService)
        {
            this.application = application;
            this.packageManagerProvider = packageManagerProvider;
            this.interactionService = interactionService;
            this.Logs = new ObservableCollection<Log>();
            this.LogsView = CollectionViewSource.GetDefaultView(this.Logs);
            this.LogsView.Filter += Filter;
            this.Sort(nameof(Log.DateTime), false);
            this.CommandHandler = new EventViewerCommandHandler(this, application);
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
                // todo: rewrite to UiCommand
                this.Progress.IsLoading = true;

                var packageManager = await this.packageManagerProvider.GetProxyFor().ConfigureAwait(false);
                
                using (var cts = new CancellationTokenSource())
                {
                    var p = new Progress<ProgressData>();
                    var task = packageManager.GetLogs(this.MaxLogs, cts.Token, p);
                    this.Progress.MonitorProgress(task, cts, p);
                    var result = await task.ConfigureAwait(false);
                    return new List<Log>(result);
                }
            }
            catch (OperationCanceledException)
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
                    // todo: initial loading of events with Uicommand
                    throw new NotImplementedException();
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

