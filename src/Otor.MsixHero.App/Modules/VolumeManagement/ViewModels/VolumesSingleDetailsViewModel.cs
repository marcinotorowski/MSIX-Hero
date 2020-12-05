using System.Collections;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes.Entities;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesSingleDetailsViewModel : NotifyPropertyChanged, INavigationAware
    {
        private AppxVolume volume;

        public AppxVolume Volume
        {
            get => this.volume;
            set => this.SetField(ref this.volume, value);
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
