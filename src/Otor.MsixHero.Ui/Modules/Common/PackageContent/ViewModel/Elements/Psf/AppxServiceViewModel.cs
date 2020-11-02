using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
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