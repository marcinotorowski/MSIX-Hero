using System;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Enums
{
    [Serializable]
    public enum AppxPackageArchitecture
    {
        x86 = 0,

        Arm = 5,

        x64 = 9,

        Neutral = 11,

        Arm64 = 12
    }
}