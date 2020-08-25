using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Hero.Executor;
using Otor.MsixHero.Ui.Modules.Common;
using Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements;
using Otor.MsixHero.Ui.Themes;
using Otor.MsixHero.Ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private readonly IBusyManager busyManager;
        private bool isActive;
        private bool firstRun = true;
        private bool isLoading;
        private int loadingProgress;
        private string loadingMessage;

        public VolumeManagerViewModel(
            IMsixHeroApplication application,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            IInteractionService interactionService,
            IConfigurationService configurationService,
            IDialogService dialogService,
            IEventAggregator eventAggregator, 
            IBusyManager busyManager)
        {
            this.application = application;
            this.interactionService = interactionService;
            this.busyManager = busyManager;


            eventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumes, ThreadOption.UIThread);
            eventAggregator.GetEvent<UiExecutedEvent<SelectVolumesCommand>>().Subscribe(this.OnSelectVolumes, ThreadOption.UIThread);
            eventAggregator.GetEvent<UiExecutedEvent<SetVolumeFilterCommand>>().Subscribe(this.OnSetVolumeFilter, ThreadOption.UIThread);
            // eventAggregator.GetEvent<VolumesVisibilityChanged>().Subscribe(this.OnVolumesVisibilityChanged, ThreadOption.UIThread);
            
            this.AllVolumesView = CollectionViewSource.GetDefaultView(this.AllVolumes);
            this.AllVolumesView.Filter = this.FilterVolume;

            this.CommandHandler = new VolumeManagerCommandHandler(this.application, volumeManagerProvider, configurationService, interactionService, dialogService, busyManager);
            this.busyManager.StatusChanged += this.BusyManagerOnStatusChanged;
        }

        private bool FilterVolume(object obj)
        {
            if (string.IsNullOrWhiteSpace(this.SearchKey))
            {
                return true;
            }

            var volume = (VolumeViewModel) obj;
            if (
                (volume.Name != null && volume.Name.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1) ||
                (volume.PackageStorePath != null &&
                 volume.PackageStorePath.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1) ||
                (volume.Label != null && volume.Label.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1))
            {
                return true;
            }

            volume.IsSelected = false;
            return false;
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.VolumeLoading)
            {
                return;
            }

            this.IsLoading = e.IsBusy;
            this.LoadingMessage = e.Message;
            this.LoadingProgress = e.Progress;
        }

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

        public VolumeManagerCommandHandler CommandHandler { get; }

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

                if (value && this.firstRun)
                {
                    this.firstRun = false;
                    this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.VolumeManager));

                    this.application.CommandExecutor
                        .WithErrorHandling(this.interactionService, true)
                        .WithBusyManager(this.busyManager, OperationType.VolumeLoading)
                        .Invoke(this, new GetVolumesCommand());
                }
            }
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.application.CommandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.VolumeManager));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public ObservableCollection<VolumeViewModel> AllVolumes { get; } = new ObservableCollection<VolumeViewModel>();

        public ICollectionView AllVolumesView { get; }

        public string Header { get; } = "Volume manager";

        public Geometry Icon { get; } = VectorIcons.TabVolumes;

        public string SearchKey
        {
            get => this.application.ApplicationState.Volumes.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetVolumeFilterCommand(value));
        }

        private void OnSetVolumeFilter(UiExecutedPayload<SetVolumeFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
            this.AllVolumesView.Refresh();
        }
        //private void OnVolumesVisibilityChanged(VolumesVisibilityChangedPayLoad visibilityInfo)
        //{
        //    for (var i = this.AllVolumes.Count - 1; i >= 0; i--)
        //    {
        //        var item = this.AllVolumes[i];
        //        if (visibilityInfo.NewHidden.Contains(item.Model))
        //        {
        //            this.AllVolumes.RemoveAt(i);
        //        }
        //    }

        //    foreach (var item in visibilityInfo.NewVisible)
        //    {
        //        this.AllVolumes.Add(new VolumeViewModel(item));
        //    }
        //}

        private void OnSelectVolumes(UiExecutedPayload<SelectVolumesCommand> obj)
        {
            if (obj.Sender is VolumeViewModel)
            {
                return;
            }

            var countSelected = 0;

            foreach (var item in this.AllVolumes)
            {
                if (this.application.ApplicationState.Volumes.SelectedVolumes.Contains(item.Model))
                {
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }

                if (item.IsSelected && countSelected < 2)
                {
                    countSelected++;
                }
            }
        }

        private void OnGetVolumes(UiExecutedPayload<GetVolumesCommand> obj)
        {
            var selectedItems = this.application.ApplicationState.Volumes.SelectedVolumes.Select(v => v.PackageStorePath).ToArray();

            this.AllVolumes.Clear();
            foreach (var item in this.application.ApplicationState.Volumes.AllVolumes)
            {
                this.AllVolumes.Add(new VolumeViewModel(this.application, item, selectedItems.Contains(item.PackageStorePath)));
            }
        }
    }
}
