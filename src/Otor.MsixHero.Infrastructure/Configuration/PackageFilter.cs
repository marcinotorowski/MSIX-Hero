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

using System;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [Flags]
    public enum PackageFilter
    {
        // ReSharper disable once ShiftExpressionRealShiftCountIsZero
        Store = 1 << 0,

        System = 1 << 1,

        Developer = 1 << 2,

        AllSources = Store | System | Developer,

        Addons = 1 << 3,

        MainApps = 1 << 4,

        MainAppsAndAddOns = MainApps | Addons,

        Installed = 1 << 5,

        Running = 1 << 6,

        InstalledAndRunning = Installed | Running,

        Neutral = 1 << 7,

        x64 = 1 << 8,

        x86 = 1 << 9,

        Arm = 1 << 10,

        Arm64 = 1 << 11,

        AllArchitectures = x64 | x86 | Neutral | Arm | Arm64,

        Default = AllArchitectures | InstalledAndRunning | Developer | Store | MainApps
    }
}