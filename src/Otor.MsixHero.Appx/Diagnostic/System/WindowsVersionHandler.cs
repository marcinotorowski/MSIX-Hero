using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using System;

namespace Otor.MsixHero.Appx.Diagnostic.System;

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