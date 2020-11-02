using Otor.MsixHero.Ui.Modules.PackageList.Navigation;
using Otor.MsixHero.Ui.ViewModel;
using Prism.Regions;

namespace Otor.MsixHero.Ui.Modules.PackageList.ViewModel.Elements
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
