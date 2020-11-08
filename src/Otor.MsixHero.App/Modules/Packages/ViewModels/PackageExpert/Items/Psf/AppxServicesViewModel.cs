using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements.Psf;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert.Items.Psf
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