using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.EventViewer.ViewModel
{
    public class EventViewerViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IApplicationStateManager stateManager;
        private readonly IInteractionService interactionService;
        private int progress;
        private string progressMessage;
        private bool isLoading;

        public EventViewerViewModel(IApplicationStateManager stateManager, IInteractionService interactionService)
        {
            this.stateManager = stateManager;
            this.interactionService = interactionService;
            var col = new ObservableCollection<Log>();
            this.LogsView = CollectionViewSource.GetDefaultView(col);
            this.Logs = new AsyncProperty<ObservableCollection<Log>>(col);
            this.Sort(nameof(Log.DateTime), false);
        }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public int Progress
        {
            get => this.progress;
            private set => this.SetField(ref this.progress, value);
        }

        public string ProgressMessage
        {
            get => this.progressMessage;
            private set => this.SetField(ref this.progressMessage, value);
        }
        
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
#pragma warning disable 4014
            this.Logs.Load(this.GetLogs());
#pragma warning restore 4014
        }

        public async Task<ObservableCollection<Log>> GetLogs()
        {
            try
            {
                this.IsLoading = true;

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
                this.IsLoading = false;
            }
        }

        public AsyncProperty<ObservableCollection<Log>> Logs { get; }

        public ICollectionView LogsView { get; }
        
        public Task Save()
        {
            return Task.FromResult(true);
        }

        public string Error => null;

        public string this[string columnName] => null;

        public bool CanSave()
        {
            return this.Error == null;
        }

        public string Title
        {
            get => "Event viewer";
        }

        public event Action<IDialogResult> RequestClose;

        public void Sort(string columnName, bool descending)
        {
            this.LogsView.SortDescriptions.Clear();
            this.LogsView.SortDescriptions.Add(new SortDescription(columnName, descending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }

        protected void OnRequestClose(IDialogResult obj)
        {
            this.RequestClose?.Invoke(obj);
        }
    }
}

