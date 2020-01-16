using System.Collections;
using System.Collections.Generic;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for SinglePackageDetails.xaml
    /// </summary>
    public partial class SinglePackageView : INavigationAware
    {
        public SinglePackageView()
        {
            InitializeComponent();
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var pkgs = navigationContext.Parameters["Packages"] as IList<string>;
            if (pkgs == null || pkgs.Count != 1)
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
