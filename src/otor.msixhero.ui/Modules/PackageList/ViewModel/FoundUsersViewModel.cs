using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class FoundUsersViewModel : NotifyPropertyChanged
    {
        public FoundUsersViewModel(List<User> listOfInstalls, ElevationStatus status)
        {
            this.InstalledBy = listOfInstalls;
            this.IsElevated = status == ElevationStatus.OK;
        }

        public bool IsElevated { get; }

        public List<User> InstalledBy { get; }
    }
}
