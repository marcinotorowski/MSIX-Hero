using otor.msixhero.lib.Domain.Appx.Manifest.Full;

namespace otor.msixhero.ui.ViewModel
{
    public class OperatingSystemViewModel : NotifyPropertyChanged
    {
        public OperatingSystemViewModel(AppxTargetOperatingSystem target)
        {
            this.Name = target.MarketingCodename == null ? target.Name : $"{target.Name} ${target.MarketingCodename}";
            this.Version = target.TechnicalVersion;
        }

        public string Name { get; }

        public string Version { get; }
    }
}
