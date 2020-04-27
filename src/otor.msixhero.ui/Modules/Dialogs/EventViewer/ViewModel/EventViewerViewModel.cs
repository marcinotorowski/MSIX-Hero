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
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.EventViewer.ViewModel
{
    public class EventViewerViewModel : NotifyPropertyChanged, IDialogAware, IDataErrorInfo
    {
        private readonly IApplicationStateManager stateManager;
        private int progress;
        private string progressMessage;
        private bool isLoading;

        public EventViewerViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
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

                var action = new GetLogs(150);
                var result = await this.stateManager.CommandExecutor.GetExecuteAsync(action).ConfigureAwait(false);
                return new ObservableCollection<Log>(result);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public AsyncProperty<ObservableCollection<Log>> Logs { get; }

        public ICollectionView LogsView { get; }
        
        public async Task Save()
        {
            var token = new Progress();

            EventHandler<ProgressData> handler = (sender, data) =>
            {
                this.Progress = data.Progress;
                this.ProgressMessage = data.Message;
            };

            this.IsLoading = true;
            try
            {
                token.ProgressChanged += handler;
            }
            finally
            {
                token.ProgressChanged -= handler;
                this.IsLoading = false;
                this.Progress = 100;
                this.ProgressMessage = null;
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                }

                return null;
            }
        }

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
    }
}

