using otor.msixhero.ui.Modules.PackageList.Navigation;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.Common.PackageContent.View
{
    /// <summary>
    /// Interaction logic for PackageContentView.xaml
    /// </summary>
    public partial class PackageContentView : INavigationAware
    {
        public PackageContentView()
        {
            InitializeComponent();
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigationParameters = new PackageListNavigation(navigationContext);
            return navigationParameters.SelectedManifests.Count == 1;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
