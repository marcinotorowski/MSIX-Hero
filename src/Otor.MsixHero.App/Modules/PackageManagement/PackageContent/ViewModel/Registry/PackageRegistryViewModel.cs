using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Registry
{
    public class PackageRegistryViewModel : NotifyPropertyChanged, IPackageContentItem, ILoadPackage
    {
        private bool _isActive;
        private string _pendingFile;

        public PackageRegistryViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public PackageContentViewType Type => PackageContentViewType.Registry;

        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (this.SetField(ref this._isActive, value) && value && this._pendingFile != null)
                {
                    this.LoadRegistry();
                }
            }
        }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            this._pendingFile = filePath;
            if (!this.IsActive)
            {
                return Task.CompletedTask;
            }

            this.LoadRegistry();
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }

        private void LoadRegistry()
        {
            if (this._pendingFile == null)
            {
                return;
            }

            this.RegistryTree = new RegistryTreeViewModel(this._pendingFile);
            this._pendingFile = null;
        }

        public RegistryTreeViewModel RegistryTree { get; private set; }

        public ICommand GoBack { get; }
    }
}
