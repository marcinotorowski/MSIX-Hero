using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Capabilities.Items;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Capabilities
{
    public class PackageCapabilitiesViewModel : PackageLazyLoadingViewModel
    {
        public PackageCapabilitiesViewModel(IPackageContentItemNavigation navigation)
        {
            this.GoBack = new DelegateCommand(() =>
            {
                navigation.SetCurrentItem(PackageContentViewType.Overview);
            });
        }
        
        public override PackageContentViewType Type => PackageContentViewType.Capabilities;
        
        public ObservableCollection<CapabilityViewModel> Capabilities { get; private set; }
        
        public ObservableCollection<CapabilityViewModel> GeneralCapabilities { get; private set; }
        
        public ObservableCollection<CapabilityViewModel> RestrictedCapabilities { get; private set; }
        
        public ObservableCollection<CapabilityViewModel> DeviceCapabilities { get; private set; }
        
        public ObservableCollection<CapabilityViewModel> CustomCapabilities { get; private set; }

        public bool HasGeneralCapabilities => this.GeneralCapabilities?.Any() == true;

        public bool HasRestrictedCapabilities => this.RestrictedCapabilities?.Any() == true;
        
        public bool HasDeviceCapabilities => this.DeviceCapabilities?.Any() == true;

        public bool HasCustomCapabilities => this.CustomCapabilities?.Any() == true;

        public ICommand GoBack { get; }

        protected override Task DoLoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.Capabilities = new ObservableCollection<CapabilityViewModel>(model.Capabilities.Select(c => new CapabilityViewModel(c)).OrderBy(c => c.SortingIndex).ThenBy(c => c.DisplayName)); ;
            this.GeneralCapabilities = new ObservableCollection<CapabilityViewModel>(model.Capabilities.Where(c => c.Type == CapabilityType.General).Select(c => new CapabilityViewModel(c)).OrderBy(c => c.SortingIndex).ThenBy(c => c.DisplayName));
            this.RestrictedCapabilities = new ObservableCollection<CapabilityViewModel>(model.Capabilities.Where(c => c.Type == CapabilityType.Restricted).Select(c => new CapabilityViewModel(c)).OrderBy(c => c.SortingIndex).ThenBy(c => c.DisplayName));
            this.CustomCapabilities = new ObservableCollection<CapabilityViewModel>(model.Capabilities.Where(c => c.Type == CapabilityType.Custom).Select(c => new CapabilityViewModel(c)).OrderBy(c => c.SortingIndex).ThenBy(c => c.DisplayName));
            this.DeviceCapabilities = new ObservableCollection<CapabilityViewModel>(model.Capabilities.Where(c => c.Type == CapabilityType.Device).Select(c => new CapabilityViewModel(c)).OrderBy(c => c.SortingIndex).ThenBy(c => c.DisplayName));
            this.OnPropertyChanged(null);
            return Task.CompletedTask;
        }
    }
}
