using System.Collections.Generic;
using Otor.MsixHero.Appx.Users;
using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.ViewModel.Elements
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
