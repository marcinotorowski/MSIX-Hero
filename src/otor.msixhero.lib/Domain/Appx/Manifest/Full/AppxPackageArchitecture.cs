using System;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
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