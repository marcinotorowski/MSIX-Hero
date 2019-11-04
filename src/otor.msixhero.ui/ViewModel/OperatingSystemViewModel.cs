using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.BusinessLayer.Models.Manifest;

namespace otor.msixhero.ui.ViewModel
{
    public class OperatingSystemViewModel : NotifyPropertyChanged
    {
        public OperatingSystemViewModel(TargetOperatingSystem target)
        {
            this.Name = target.MarketingCodename == null ? target.Name : $"{target.Name} ${target.MarketingCodename}";
            this.Version = target.TechnicalVersion;
        }

        public string Name { get; }

        public string Version { get; }
    }
}
