using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.Source;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation
{
    public class PackageInstallationViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        private bool _isActive;

        public PackageInstallationViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        public ICommand GoBack { get; }

        public PackageContentViewType Type { get; } = PackageContentViewType.Installation;

        public bool IsActive
        {
            get => _isActive;
            set => this.SetField(ref this._isActive, value);
        }

        public PackageSourceViewModel Source { get; private set; }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            this.Source = model.Source switch
            {
                NotInstalledSource nis => new NotInstalledSourceViewModel(nis),
                AppInstallerPackageSource appInstaller => new AppInstallerSourceViewModel(appInstaller),
                DeveloperSource developer => new DeveloperSourceViewModel(developer),
                StandardSource standard => new StandardSourceViewModel(standard),
                StorePackageSource store => new StorePackageSourceViewModel(store),
                SystemSource system => new SystemSourceViewModel(system),
                _ => null
            };

            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
    }
}
