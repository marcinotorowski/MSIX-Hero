using System;

namespace Otor.MsixHero.Appx.Packaging.Packer.Enums
{
    [Flags]
    public enum AppxPackerOptions
    {
        NoCompress = 1,
        NoValidation = 1 << 1
    }
}