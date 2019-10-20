using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Installed.ViewModel;

namespace MSI_Hero.ViewModel
{
    public class ApplicationState : NotifyPropertyChanged
    {
        public ApplicationState(InstalledViewModel installed)
        {
            Installed = installed;
        }

        public InstalledViewModel Installed { get; private set; }
    }
}
