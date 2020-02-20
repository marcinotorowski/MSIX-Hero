using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Helpers;
using otor.msixhero.ui.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.Volumes.ViewModel
{
    public class VolumesViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly IAppxVolumeManager volumeManager;
        private readonly IInteractionService interactionService;
        private AppxVolume selectedVolume;

        public VolumesViewModel(IAppxVolumeManager volumeManager, IInteractionService interactionService)
        {
            this.volumeManager = volumeManager;
            this.interactionService = interactionService;
            this.AllVolumes = new AsyncProperty<ObservableCollection<AppxVolume>>(this.LoadVolumes());
        }

        public AsyncProperty<ObservableCollection<AppxVolume>> AllVolumes { get; }

        public AppxVolume SelectedVolume
        {
            get => this.selectedVolume;
            set => this.SetField(ref this.selectedVolume, value);
        }

        public string Title { get; } = "Volume manager";

        bool IDialogAware.CanCloseDialog()
        {
            return true;
        }

        void IDialogAware.OnDialogClosed()
        {
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {

        }

        public event Action<IDialogResult> RequestClose;

        private async Task<ObservableCollection<AppxVolume>> LoadVolumes()
        {
            var items = await this.volumeManager.GetAll().ConfigureAwait(false);
            var defaultVolume = await this.volumeManager.GetDefault().ConfigureAwait(false);

            if (defaultVolume != null)
            {
                defaultVolume = items.FirstOrDefault(v => string.Equals(v.PackageStorePath, defaultVolume.PackageStorePath, StringComparison.OrdinalIgnoreCase));
                if (defaultVolume != null)
                {
                    defaultVolume.IsDefault = true;
                }
            }

            return new ObservableCollection<AppxVolume>(items);
        }
    }
}

