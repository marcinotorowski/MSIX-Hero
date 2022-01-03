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

using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public enum DefaultScreen
    {
        Dashboard = 0,
        Packages = 1,
        Volumes = 2,
        Events = 3,
        System = 4
    }
    
    public enum UxTierLevel
    {
        Auto = -1,
        Basic = 0,
        Medium = 1,
        Rich = 2,
    }
    
    [DataContract]
    public class UiConfiguration : BaseJsonSetting
    {
        public UiConfiguration()
        {
            this.ConfirmDeletion = true;
            this.DefaultScreen = DefaultScreen.Packages;
        }

        [DataMember(Name = "confirmDeletion")]
        public bool ConfirmDeletion { get; set; }

        [DataMember(Name = "defaultScreen")]
        public DefaultScreen DefaultScreen { get; set; }

        /// <summary>
        /// The UI level. Can be 0, 1 or 2, where 2 is the beauty, and 0 is the performance. Any other value should be treated as auto (based on system performance).
        /// </summary>
        [DataMember(Name = "uxTier")]
        public UxTierLevel UxTier { get; set; } = UxTierLevel.Auto;
    }
}
