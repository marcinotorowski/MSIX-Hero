// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxTargetOperatingSystem
    {
        public string Name { get; set; }
        
        public string NativeFamilyName { get; set; }

        public string TechnicalVersion { get; set; }
        
        public string MarketingCodename { get; set; }

        public AppxTargetOperatingSystemType IsNativeMsixPlatform { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.MarketingCodename) ? $"{this.Name} ({this.TechnicalVersion})" : $"{this.Name} {this.MarketingCodename} ({this.TechnicalVersion})";
        }
    }
}
