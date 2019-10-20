using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using MSI_Hero.Modules.Installed.ViewModel;
using MSI_Hero.Services;
using otor.msihero.lib;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace MSI_Hero.ViewModel
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;
        private bool isLoading;
        private string loadingMessage;
        private int loadingProgress;
        private IDialogService dialogService;
        private readonly InstalledViewModel installedPackages;

        public MainViewModel(
            IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IDialogService dialogService, 
            IBusyManager busyManager,
            InstalledViewModel installedPackages)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            this.dialogService = dialogService;
            this.installedPackages = installedPackages;
            this.Tools = new ObservableCollection<ToolViewModel>();
            this.Tools.Add(new ToolViewModel("notepad.exe"));
            this.Tools.Add(new ToolViewModel("regedit.exe"));
            this.Tools.Add(new ToolViewModel("powershell.exe"));

            busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.InstalledPackages = installedPackages;
        }

        public InstalledViewModel InstalledPackages { get; }

        public CommandHandler CommandHandler => new CommandHandler(this.eventAggregator, this.regionManager, this.dialogService);

        public ObservableCollection<ToolViewModel> Tools { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }

        public int LoadingProgress
        {
            get => this.loadingProgress;
            private set => this.SetField(ref this.loadingProgress, value);
        }

        public string LoadingMessage
        {
            get => this.loadingMessage;
            private set => this.SetField(ref this.loadingMessage, value);
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            this.IsLoading = e.IsBusy;
            this.LoadingMessage = e.Message;
            this.LoadingProgress = e.Progress;
        }
    }
}
