using System.Collections;
using System.Linq;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesSingleDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private readonly IMsixHeroApplication application;
        private AppxVolume volume;

        public VolumesSingleDetailsViewModel(IMsixHeroApplication application)
        {
            this.application = application;
            application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumes);
        }

        public AppxVolume Volume
        {
            get => this.volume;
            set => this.SetField(ref this.volume, value);
        }

        private void OnGetVolumes(UiExecutedPayload<GetVolumesCommand> obj)
        {
            return;
            if (this.application.ApplicationState.Volumes.SelectedVolumes.Count == 1)
            {
                this.Volume = this.application.ApplicationState.Volumes.SelectedVolumes.First();
            }
            else
            {
                this.Volume = null;
            }
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.First().Value is IEnumerable pkgs)
            {
                this.Volume = pkgs.OfType<AppxVolume>().FirstOrDefault();
            }
            else if (navigationContext.Parameters.First().Value is AppxVolume ip)
            {
                this.Volume = ip;
            }
            else
            {
                this.Volume = null;
            }
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
