﻿using System;

namespace otor.msixhero.lib.Domain.Appx.Packages
{
    [Flags]
    public enum PackageFilter
    {
        // ReSharper disable once ShiftExpressionRealShiftCountIsZero
        Store = 1 << 0,

        System = 1 << 1,

        Developer = 1 << 2,
        
        All = Store | System | Developer
    }
}