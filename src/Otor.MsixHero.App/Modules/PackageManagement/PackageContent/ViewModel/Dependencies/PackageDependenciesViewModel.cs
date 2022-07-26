using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies
{
    public class PackageDependenciesViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        public PackageDependenciesViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        private bool _isActive;

        public PackageContentViewType Type => PackageContentViewType.Dependencies;

        public bool IsActive
        {
            get => this._isActive;
            set => this.SetField(ref this._isActive, value);
        }

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

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.SoftwareDependencies = new ObservableCollection<SoftwareDependencyViewModel>(model.PackageDependencies.Select(d => new SoftwareDependencyViewModel(d)));
            this.SystemDependencies = new ObservableCollection<SystemDependencyViewModel>(model.OperatingSystemDependencies.Select(d => new SystemDependencyViewModel(d)));
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
        
        public ICommand GoBack { get; }
    }
}
