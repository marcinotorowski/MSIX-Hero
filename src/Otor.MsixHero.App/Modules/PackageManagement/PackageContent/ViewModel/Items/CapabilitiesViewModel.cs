// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items
{
    public class CapabilitiesViewModel
    {
        public CapabilitiesViewModel(IEnumerable<AppxCapability> capabilities)
        {
            this.Count = 0;
            foreach (var c in capabilities.GroupBy(c => c.Type))
            {
                switch (c.Key)
                {
                    case CapabilityType.General:
                        this.General = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.General.Count;
                        break;
                    case CapabilityType.Restricted:
                        this.Restricted = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Restricted.Count;
                        break;
                    case CapabilityType.Device:
                        this.Device = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Device.Count;
                        break;
                    default:
                        this.Custom = new List<CapabilityViewModel>(c.Select(cap => new CapabilityViewModel(cap)));
                        this.Count += this.Custom.Count;
                        break;
                }
            }
        }

        public int Count { get; }

        public IReadOnlyCollection<CapabilityViewModel> General { get; }

        public IReadOnlyCollection<CapabilityViewModel> Restricted { get; }

        public IReadOnlyCollection<CapabilityViewModel> Device { get; }

        public IReadOnlyCollection<CapabilityViewModel> Custom { get; }

        public bool HasGeneral => this.General?.Any() == true;

        public bool HasDevice => this.Device?.Any() == true;

        public bool HasCustom => this.Custom?.Any() == true;

        public bool HasRestricted => this.Restricted?.Any() == true;
    }
}