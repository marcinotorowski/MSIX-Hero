using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Psf
{
    public class AppxServicesViewModel : NotifyPropertyChanged
    {
        public AppxServicesViewModel(IEnumerable<AppxExtension> extensions)
        {
            this.Services = new ObservableCollection<AppxServiceViewModel>(extensions.OfType<AppxService>().Select(e => new AppxServiceViewModel(e)));
            this.HasServices = this.Services.Count > 0;
        }

        public bool HasServices { get; set; }

        public ObservableCollection<AppxServiceViewModel> Services { get; }
    }
}