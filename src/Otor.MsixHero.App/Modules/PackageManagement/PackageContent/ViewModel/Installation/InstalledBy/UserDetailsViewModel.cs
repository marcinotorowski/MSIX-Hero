using System.Collections.Generic;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Users;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Installation.InstalledBy
{
    public class UserDetailsViewModel : NotifyPropertyChanged
    {

        public UserDetailsViewModel(List<User> listOfInstalls, ElevationStatus status)
        {
            this.InstalledBy = listOfInstalls;
            this.IsElevated = status == ElevationStatus.Ok;
        }

        public bool IsElevated { get; }

        public List<User> InstalledBy { get; }
    }
}
