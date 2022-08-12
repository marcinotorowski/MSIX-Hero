using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.Source
{
    public class AppInstallerSourceViewModel : PackageSourceViewModel
    {
        public AppInstallerSourceViewModel(AppInstallerPackageSource src) : base(src)
        {
        }

        public string AppInstallerUri => ((AppInstallerPackageSource)this.Src).AppInstallerUri?.ToString();
    }
}
