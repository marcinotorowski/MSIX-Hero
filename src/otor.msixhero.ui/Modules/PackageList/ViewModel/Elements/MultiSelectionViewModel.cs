using otor.msixhero.ui.Modules.PackageList.Navigation;
using otor.msixhero.ui.ViewModel;
using Prism.Regions;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class MultiSelectionViewModel : NotifyPropertyChanged, INavigationAware
    {
        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var navigation = new PackageListNavigation(navigationContext);

            if (navigation.SelectedManifests?.Count < 2)
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
