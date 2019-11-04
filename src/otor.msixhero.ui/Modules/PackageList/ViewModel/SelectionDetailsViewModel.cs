using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class SelectionDetailsViewModel : NotifyPropertyChanged
    {
        private readonly SelectionDetails model;

        public SelectionDetailsViewModel(SelectionDetails model)
        {
            this.model = model;
        }

        public bool IsElevated => this.model.InstalledOn.Status == ElevationStatus.OK;

        public List<User> InstalledBy
        {
            get => this.model.InstalledOn.Users;
        }
    }
}
