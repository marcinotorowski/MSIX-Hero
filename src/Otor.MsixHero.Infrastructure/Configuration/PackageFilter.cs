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