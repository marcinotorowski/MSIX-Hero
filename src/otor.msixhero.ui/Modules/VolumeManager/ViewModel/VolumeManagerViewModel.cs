using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using otor.msixhero.ui.Themes;
using otor.msixhero.ui.ViewModel;
using Prism;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel
{
    public class VolumeManagerViewModel : NotifyPropertyChanged, INavigationAware, IHeaderViewModel, IActiveAware
    {
        private readonly IApplicationStateManager stateManager;
        private bool isActive;
        private string searchKey;

        public VolumeManagerViewModel(IApplicationStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.AllVolumes = new AsyncProperty<ObservableCollection<AppxVolume>>();
        }

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
#pragma warning disable 4014
                    if (!this.AllVolumes.HasValue)
                    {
                        this.AllVolumes.Load(this.LoadVolumes());
                    }
#pragma warning restore 4014
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

        public AsyncProperty<ObservableCollection<AppxVolume>> AllVolumes { get; }

        public string Header { get; } = "Volume manager";

        public Geometry Icon { get; } = VectorIcons.TabVolumes;

        public string SearchKey
        {
            get => this.searchKey;
            set => this.SetField(ref this.searchKey, value);
        }

        private async Task<ObservableCollection<AppxVolume>> LoadVolumes()
        {
            var items = await this.stateManager.CommandExecutor.GetExecuteAsync(new GetVolumes()).ConfigureAwait(false);
            return new ObservableCollection<AppxVolume>(items);
        }
    }
}
