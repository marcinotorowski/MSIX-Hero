using System.Collections.ObjectModel;
using System.Linq;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class ManifestDetailsViewModel : NotifyPropertyChanged
    {
        public ManifestDetailsViewModel(AppxManifestSummary model, IApplicationState applicationState)
        {
            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();

            foreach (var item in model.OperatingSystemDependencies)
            {
                this.OperatingSystemDependencies.Add(new OperatingSystemDependencyViewModel(item));
            }

            foreach (var item in model.PackageDependencies)
            {
                var foundMatching = applicationState.Packages.VisibleItems.FirstOrDefault(pkg => pkg.Name == item.Name && pkg.Publisher == item.Publisher) ?? applicationState.Packages.HiddenItems.FirstOrDefault(pkg => pkg.Name == item.Name && pkg.Publisher == item.Publisher);
                this.PackageDependencies.Add(new PackageDependencyViewModel(item, foundMatching?.DisplayName, foundMatching?.DisplayPublisherName));
            }
        }

        public ObservableCollection<OperatingSystemDependencyViewModel> OperatingSystemDependencies { get; }

        public ObservableCollection<PackageDependencyViewModel> PackageDependencies { get; }
    }
}
