using System;

namespace Otor.MsixHero.Appx.Common.WindowsVersioning;

public record WindowsVersionHandler(VersionRange VersionRange, Func<Version, AppxTargetOperatingSystem> NameGenerator) : IComparable<WindowsVersionHandler>
{
    public int CompareTo(WindowsVersionHandler other)
    {
        if (other == null)
        {
            return 1;
        }

        return this.VersionRange.CompareTo(other.VersionRange);
    }
}