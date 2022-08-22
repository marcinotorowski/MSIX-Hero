// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items
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
