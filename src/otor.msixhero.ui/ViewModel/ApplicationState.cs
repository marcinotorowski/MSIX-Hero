using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Installed.ViewModel;

namespace MSI_Hero.ViewModel
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
