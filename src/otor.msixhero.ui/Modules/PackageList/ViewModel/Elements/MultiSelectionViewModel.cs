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
        
        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var param = navigationContext.Parameters["Packages"] as IList<string>;
            if (param == null || param.Count < 2)
            {
                return;
            }

            this.packageFullNames = param;
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var param = navigationContext.Parameters["Packages"] as IList<string>;
            if (param == null || param.Count < 2)
            {
                return false;
            }

            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
