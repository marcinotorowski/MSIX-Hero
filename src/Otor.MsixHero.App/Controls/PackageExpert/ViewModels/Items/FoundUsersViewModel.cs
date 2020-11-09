using System.Collections.Generic;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Lib.Domain.State;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items
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
