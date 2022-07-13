using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.Source
{
    public class AppInstallerSourceViewModel : PackageSourceViewModel
    {
        public AppInstallerSourceViewModel(AppInstallerPackageSource src) : base(src)
        {
            this.AppInstallerUri = src.AppInstallerUri?.ToString();
        }

        public string AppInstallerUri { get; }
    }
}
