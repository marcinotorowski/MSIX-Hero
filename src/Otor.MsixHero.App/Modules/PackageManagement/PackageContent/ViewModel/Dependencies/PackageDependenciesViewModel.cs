using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies.Items;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies
{
    public class PackageDependenciesViewModel : PackageLazyLoadingViewModel
    {
        public PackageDependenciesViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Dependencies;
        
        public ObservableCollection<SoftwareDependencyViewModel> SoftwareDependencies { get; private set; }

        public ObservableCollection<SystemDependencyViewModel> SystemDependencies { get; private set; }

        public bool HasWindows11Dependency
        {
            get
            {
                return this.SystemDependencies.Any(sd => sd.MinimumWindowsVersionType == WindowsVersion.Win11 || sd.TestedVersionType == WindowsVersion.Win11);
            }
        }

        public bool HasWindows10Dependency
        {
            get
            {
                return this.SystemDependencies.Any(sd => sd.MinimumWindowsVersionType == WindowsVersion.Win10 || sd.TestedVersionType == WindowsVersion.Win10);
            }
        }

        public bool HasSoftwareDependencies => this.SoftwareDependencies?.Any() == true;
        
        public bool HasSystemDependencies => this.SystemDependencies?.Any() == true;

        protected override Task DoLoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.SoftwareDependencies = new ObservableCollection<SoftwareDependencyViewModel>(model.PackageDependencies.Select(d => new SoftwareDependencyViewModel(d)));
            this.SystemDependencies = new ObservableCollection<SystemDependencyViewModel>(model.OperatingSystemDependencies.Select(d => new SystemDependencyViewModel(d)));
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
        
        public ICommand GoBack { get; }
    }
}
