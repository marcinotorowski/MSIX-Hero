using otor.msixhero.ui.Modules.PackageList.ViewModel;

namespace otor.msixhero.ui.ViewModel
{
    public class ApplicationState : NotifyPropertyChanged
    {
        public ApplicationState(PackageListViewModel installed)
        {
            Installed = installed;
        }

        public PackageListViewModel Installed { get; private set; }
    }
}
