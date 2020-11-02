using System;

namespace Otor.MsixHero.Appx.Packaging.Installation.Enums
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