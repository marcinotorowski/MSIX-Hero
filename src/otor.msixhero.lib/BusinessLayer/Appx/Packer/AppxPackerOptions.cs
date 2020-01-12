using System;

namespace otor.msixhero.lib.BusinessLayer.Appx.Packer
{
    [Flags]
    public enum AppxPackerOptions
    {
        NoCompress = 1,
        NoValidation = 1 << 1
    }
}