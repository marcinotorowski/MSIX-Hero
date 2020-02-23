using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel.Elements
{
    public class OperatingSystemDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxOperatingSystemDependency model;

        public OperatingSystemDependencyViewModel(AppxOperatingSystemDependency model)
        {
            this.model = model;
        }

        public string MinimumDisplayName
        {
            get
            {
                if (this.model.Minimum == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(this.model.Minimum.MarketingCodename))
                {
                    return this.model.Minimum.Name;
                }

                return $"{this.model.Minimum.Name} ({this.model.Minimum.MarketingCodename})";
            }
        }

        public string MinimumVersion
        {
            get
            {
                if (this.model.Minimum == null)
                {
                    return null;
                }

                return this.model.Minimum.TechnicalVersion;
            }
        }

        public string TestedDisplayName
        {
            get
            {
                if (this.model.Tested == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(this.model.Tested.MarketingCodename))
                {
                    return this.model.Tested.Name;
                }

                return $"{this.model.Tested.Name} ({this.model.Tested.MarketingCodename})";
            }
        }

        public string TestedVersion
        {
            get
            {
                if (this.model.Tested == null)
                {
                    return null;
                }

                return this.model.Tested.TechnicalVersion;
            }
        }

        public bool HasMinimumVersion => this.model.Minimum != null;

        public bool HasTestedVersion => this.model.Tested != null;
    }
}
