using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry.Items;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry
{
    public class PackageRegistryViewModel : PackageLazyLoadingViewModel
    {
        private bool _isVirtualizationDisabled;
        public PackageRegistryViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }

        public bool IsVirtualizationDisabled
        {
            get => this._isVirtualizationDisabled;
            private set => this.SetField(ref this._isVirtualizationDisabled, value);
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Registry;
        
        public RegistryTreeViewModel RegistryTree { get; private set; }

        public ICommand GoBack { get; }

        protected override Task DoLoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            this.RegistryTree = new RegistryTreeViewModel(filePath);
            this.IsVirtualizationDisabled = !model.RegistryVirtualizationEnabled;
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
    }
}
