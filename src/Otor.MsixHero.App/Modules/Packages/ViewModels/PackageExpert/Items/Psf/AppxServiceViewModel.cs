using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert.Items.Psf
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