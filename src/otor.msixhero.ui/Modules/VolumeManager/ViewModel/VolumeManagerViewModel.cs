using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Modules.VolumeManager.ViewModel.Elements;
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
        private readonly IBusyManager busyManager;
        private bool isActive;
        private bool firstRun = true;

        public VolumeManagerViewModel(IApplicationStateManager stateManager, IEventAggregator eventAggregator, IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.busyManager = busyManager;
            eventAggregator.GetEvent<VolumesCollectionChanged>().Subscribe(this.OnVolumesCollectionChanged, ThreadOption.UIThread);
            eventAggregator.GetEvent<VolumesSelectionChanged>().Subscribe(this.OnVolumesSelectionChanged, ThreadOption.UIThread);
            eventAggregator.GetEvent<VolumesFilterChanged>().Subscribe(this.OnVolumesFilterChanged, ThreadOption.UIThread);
            eventAggregator.GetEvent<VolumesVisibilityChanged>().Subscribe(this.OnVolumesVisibilityChanged, ThreadOption.UIThread);
            this.AllVolumesView = CollectionViewSource.GetDefaultView(this.AllVolumes);
            this.CommandHandler = new VolumeManagerCommandHandler(stateManager, busyManager);
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

                        var context = this.busyManager.Begin();
                        this.stateManager.CommandExecutor.GetExecuteAsync(new GetVolumes()).ContinueWith(
                            t =>
                            {
                                this.busyManager.End(context);
                            },
                            TaskContinuationOptions.AttachedToParent);
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

        public ObservableCollection<VolumeViewModel> AllVolumes { get; } = new ObservableCollection<VolumeViewModel>();

        public ICollectionView AllVolumesView { get; }

        public string Header { get; } = "Volume manager";

        public Geometry Icon { get; } = VectorIcons.TabVolumes;

        public string SearchKey
        {
            get => this.stateManager.CurrentState.Volumes.SearchKey;
            set => this.stateManager.CommandExecutor.ExecuteAsync(SetVolumeFilter.CreateFrom(value));
        }

        private void OnVolumesFilterChanged(VolumesFilterChangedPayload filter)
        {
            if (filter.NewSearchKey != filter.OldSearchKey)
            {
                this.OnPropertyChanged(nameof(SearchKey));
            }
        }
        private void OnVolumesVisibilityChanged(VolumesVisibilityChangedPayLoad visibilityInfo)
        {
            for (var i = this.AllVolumes.Count - 1; i >= 0; i--)
            {
                var item = this.AllVolumes[i];
                if (visibilityInfo.NewHidden.Contains(item.Model))
                {
                    this.AllVolumes.RemoveAt(i);
                }
            }

            foreach (var item in visibilityInfo.NewVisible)
            {
                this.AllVolumes.Add(new VolumeViewModel(item, this.stateManager));
            }
        }

        private void OnVolumesSelectionChanged(VolumesSelectionChangedPayLoad selectionInfo)
        {
            var countSelected = 0;

            foreach (var item in this.AllVolumes)
            {
                if (selectionInfo.Selected.Contains(item.Model))
                {
                    item.IsSelected = true;
                }
                else if (selectionInfo.Unselected.Contains(item.Model))
                {
                    item.IsSelected = false;
                }

                if (item.IsSelected && countSelected < 2)
                {
                    countSelected++;
                }
            }

            // TODO
            //var selected = this.AllVolumes.Where(p => p.IsSelected).Select(p => p.PackageStorePath).ToArray();
            //switch (selected.Length)
            //{
            //    case 0:
            //    {
            //        this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(VolumesModule.SidebarEmptySelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
            //        break;
            //    }

            //    case 1:
            //    {
            //        this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarSingleSelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
            //        break;
            //    }

            //    default:
            //    {
            //        this.regionManager.Regions["PackageSidebar"].RequestNavigate(new Uri(PackageListModule.SidebarMultiSelection, UriKind.Relative), new NavigationParameters { { "Packages", selected } });
            //        break;
            //    }
            //}
        }

        private void OnVolumesCollectionChanged(VolumesCollectionChangedPayLoad obj)
        {
            var selectedItems = this.AllVolumes.Where(a => a.IsSelected).Select(a => a.Model.PackageStorePath).ToArray();
            if (obj.Type == CollectionChangeType.Reset)
            {
                this.AllVolumes.Clear();
                foreach (var item in this.stateManager.CurrentState.Volumes.VisibleItems.Union(this.stateManager.CurrentState.Volumes.HiddenItems))
                {
                    this.AllVolumes.Add(new VolumeViewModel(item, this.stateManager, selectedItems.Contains(item.PackageStorePath)));
                }
            }
            else
            {
                foreach (var item in obj.NewVolumes)
                {
                    this.AllVolumes.Add(new VolumeViewModel(item, this.stateManager, selectedItems.Contains(item.PackageStorePath)));
                }

                foreach (var item in obj.OldVolumes)
                {
                    var found = this.AllVolumes.FirstOrDefault(a => a.Model == item);
                    if (found != null)
                    {
                        this.AllVolumes.Remove(found);
                    }
                }
            }
        }
    }
}
