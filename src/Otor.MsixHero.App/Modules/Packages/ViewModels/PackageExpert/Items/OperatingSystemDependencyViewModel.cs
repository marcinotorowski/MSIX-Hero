using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.App.Modules.Packages.ViewModels.PackageExpert.Items
{
    public class OperatingSystemDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxOperatingSystemDependency model;

        public OperatingSystemDependencyViewModel(AppxOperatingSystemDependency model)
        {
            this.model = model;
        }

        public AppxTargetOperatingSystemType IsMsixNativeSupported
        {
            get => this.model.Minimum?.IsNativeMsixPlatform ?? this.model.Tested?.IsNativeMsixPlatform ?? AppxTargetOperatingSystemType.Other;
        }

        public string FamilyName
        {
            get => this.model.Minimum?.NativeFamilyName ?? this.model.Tested?.NativeFamilyName;
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
