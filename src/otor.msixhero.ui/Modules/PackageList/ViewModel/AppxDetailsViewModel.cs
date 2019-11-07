using System.Collections.ObjectModel;
using System.Linq;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Summary;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class AppxDetailsViewModel : NotifyPropertyChanged
    {
        public AppxDetailsViewModel(AppxPackage model)
        {
            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();
            this.DisplayName = model.DisplayName;
            this.Version = model.Version.ToString();
            this.Logo = model.Logo;
            this.FamilyName = model.FamilyName;

            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();

            if (model.OperatingSystemDependencies != null)
            {
                foreach (var item in model.OperatingSystemDependencies)
                {
                    this.OperatingSystemDependencies.Add(new OperatingSystemDependencyViewModel(item));
                }
            }

            if (model.PackageDependencies != null)
            {
                foreach (var item in model.PackageDependencies)
                {
                    this.PackageDependencies.Add(new PackageDependencyViewModel(item));
                }
            }
        }

        public string DisplayName { get; }

        public string Logo { get; }

        public string FamilyName { get; }

        public string Version { get; }

        public ObservableCollection<OperatingSystemDependencyViewModel> OperatingSystemDependencies { get; }

        public ObservableCollection<PackageDependencyViewModel> PackageDependencies { get; }
    }
}
