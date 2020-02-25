using System.Collections.Generic;
using otor.msixhero.ui.Modules.PackageList.Navigation;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for EmptySelectionView.xaml
    /// </summary>
    public partial class EmptySelectionView : INavigationAware
    {
        public EmptySelectionView()
        {
            InitializeComponent();
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigationParameters = new PackageListNavigation(navigationContext);
            return navigationParameters.SelectedManifests.Count == 0;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
