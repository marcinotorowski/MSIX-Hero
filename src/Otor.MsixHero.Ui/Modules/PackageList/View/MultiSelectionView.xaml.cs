using Otor.MsixHero.Ui.Modules.PackageList.Navigation;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for MultiSelectionView.xaml
    /// </summary>
    public partial class MultiSelectionView : INavigationAware
    {
        public MultiSelectionView()
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
