using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.ui.ViewModel;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class MultiSelectionViewModel : NotifyPropertyChanged, INavigationAware
    {
        private IList<string> packageFullNames = new List<string>();

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.packageFullNames = ((IEnumerable<string>)navigationContext.Parameters[nameof(InstalledPackage.PackageId)]).ToList();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return ((IEnumerable<string>)navigationContext.Parameters[nameof(InstalledPackage.PackageId)]).SequenceEqual(this.packageFullNames);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
