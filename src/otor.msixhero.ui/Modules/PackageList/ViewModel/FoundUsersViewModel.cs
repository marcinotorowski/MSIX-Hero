using System.Collections.Generic;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class FoundUsersViewModel : NotifyPropertyChanged
    {
        private readonly FoundUsers model;

        public FoundUsersViewModel(FoundUsers model)
        {
            this.model = model;
        }

        public bool IsElevated => this.model.Status == ElevationStatus.OK;

        public List<User> InstalledBy
        {
            get => this.model.Users;
        }
    }
}
