using Otor.MsixHero.Ui.Modules.PackageList.ViewModel;

namespace Otor.MsixHero.Ui.ViewModel
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
