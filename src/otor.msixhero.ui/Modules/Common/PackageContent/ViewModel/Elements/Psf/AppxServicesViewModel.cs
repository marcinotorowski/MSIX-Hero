using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
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