using System;

namespace otor.msixhero.lib.Domain.Appx.Packages
{
    [Flags]
    public enum MsixPackageType
    {
        Uwp = 1,
        BridgeDirect = 2,
        BridgePsf = 4,
        Web = 8,
        Framework = 16
    }
}