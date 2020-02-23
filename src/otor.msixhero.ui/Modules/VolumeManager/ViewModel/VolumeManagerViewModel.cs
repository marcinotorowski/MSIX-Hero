using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Themes;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Events;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private readonly IApplicationStateManager stateManager;
        private bool isActive;
        private string searchKey;
        private bool firstRun = true;

        public VolumeManagerViewModel(IApplicationStateManager stateManager, IEventAggregator eventAggregator)
        {
            this.stateManager = stateManager;
            eventAggregator.GetEvent<VolumesCollectionChanged>().Subscribe(this.OnVolumesCollectionChanged, ThreadOption.UIThread);
            this.AllVolumesView = CollectionViewSource.GetDefaultView(this.AllVolumes);
            this.CommandHandler = new VolumeManagerCommandHandler(stateManager);
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

                if (value)
                {
                    this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.VolumeManager));

                    if (this.firstRun)
                    {
                        this.firstRun = false;
                        this.stateManager.CommandExecutor.ExecuteAsync(new GetVolumes());
                    }
                }
            }
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.stateManager.CommandExecutor.ExecuteAsync(new SetMode(ApplicationMode.VolumeManager));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        public ObservableCollection<AppxVolume> AllVolumes { get; } = new ObservableCollection<AppxVolume>();

        public ICollectionView AllVolumesView { get; }

        public string Header { get; } = "Volume manager";

        public Geometry Icon { get; } = VectorIcons.TabVolumes;

        public string SearchKey
        {
            get => this.searchKey;
            set => this.SetField(ref this.searchKey, value);
        }
        private void OnVolumesCollectionChanged(VolumesCollectionChangedPayLoad obj)
        {
            if (obj.Type == CollectionChangeType.Reset)
            {

                this.AllVolumes.Clear();
                foreach (var item in this.stateManager.CurrentState.Volumes.VisibleItems.Union(this.stateManager.CurrentState.Volumes.HiddenItems))
                {
                    this.AllVolumes.Add(item);
                }
            }
            else
            {
                foreach (var item in obj.NewVolumes)
                {
                    this.AllVolumes.Add(item);
                }

                foreach (var item in obj.OldVolumes)
                {
                    this.AllVolumes.Remove(item);
                }
            }
        }
    }
}
