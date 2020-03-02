using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
{
    public class AppxServiceViewModel : NotifyPropertyChanged
    {
        private readonly AppxService service;

        public AppxServiceViewModel(AppxService service)
        {
            this.service = service;
        }

        public string Name => this.service.Name;
    }
}