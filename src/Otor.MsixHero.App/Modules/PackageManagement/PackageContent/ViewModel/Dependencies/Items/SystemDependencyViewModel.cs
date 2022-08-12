
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

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Dependencies.Items
{
    public class SystemDependencyViewModel : NotifyPropertyChanged
    {
        private readonly AppxOperatingSystemDependency _model;

        public SystemDependencyViewModel(AppxOperatingSystemDependency model)
        {
            _model = model;
        }

        public WindowsVersion MinimumWindowsVersionType => _model.Minimum?.WindowsVersion ?? WindowsVersion.Unspecified;

        public WindowsVersion TestedVersionType => _model.Tested?.WindowsVersion ?? WindowsVersion.Unspecified;

        public AppxTargetOperatingSystemType IsMsixNativeSupported => _model.Minimum?.IsNativeMsixPlatform ?? _model.Tested?.IsNativeMsixPlatform ?? AppxTargetOperatingSystemType.Other;

        public string FamilyName => _model.Minimum?.NativeFamilyName ?? _model.Tested?.NativeFamilyName;

        public string MinimumDisplayName
        {
            get
            {
                if (_model.Minimum == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(_model.Minimum.MarketingCodename))
                {
                    return _model.Minimum.Name;
                }

                return $"{_model.Minimum.Name} ({_model.Minimum.MarketingCodename})";
            }
        }

        public string MinimumVersion
        {
            get
            {
                if (_model.Minimum == null)
                {
                    return null;
                }

                return _model.Minimum.TechnicalVersion;
            }
        }

        public string TestedDisplayName
        {
            get
            {
                if (_model.Tested == null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(_model.Tested.MarketingCodename))
                {
                    return _model.Tested.Name;
                }

                return $"{_model.Tested.Name} ({_model.Tested.MarketingCodename})";
            }
        }

        public string TestedVersion
        {
            get
            {
                if (_model.Tested == null)
                {
                    return null;
                }

                return _model.Tested.TechnicalVersion;
            }
        }

        public bool HasMinimumVersion => _model.Minimum != null;

        public bool HasTestedVersion => _model.Tested != null;
    }
}