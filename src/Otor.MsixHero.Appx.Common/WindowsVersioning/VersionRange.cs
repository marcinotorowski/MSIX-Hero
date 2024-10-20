#nullable enable
using System;
using System.Linq;

namespace Otor.MsixHero.Appx.Common.WindowsVersioning;

public class VersionRange : IComparable<VersionRange>
{
    private VersionRange(Version? lower, Version? upper, bool lowerIncluded, bool upperIncluded)
    {
        this.LowerRange = lower;
        this.UpperRange = upper;
        this.LowerVersionIncluded = lowerIncluded;
        this.UpperVersionIncluded = upperIncluded;
    }

    private VersionRange(Version lower) : this(lower, lower, true, true)
    {
    }

    public static VersionRange Exact(Version version)
    {
        return new VersionRange(version);
    }

    public static VersionRange Exact(string version)
    {
        return Exact(Version.Parse(version));
    }

    public static VersionRange Between(Version versionLower, bool includeLower, Version versionUpper, bool includeUpper)
    {
        return new VersionRange(versionLower, versionUpper, includeLower, includeUpper);
    }

    public static VersionRange Between(string versionLower, bool includeLower, string versionUpper, bool includeUpper)
    {
        return Between(Version.Parse(versionLower), includeLower, Version.Parse(versionUpper), includeUpper);
    }

    public static VersionRange HigherThan(Version versionLower)
    {
        return new VersionRange(versionLower, null, false, false);
    }

    public static VersionRange HigherThan(string versionLower)
    {
        return HigherThan(Version.Parse(versionLower));
    }

    public static VersionRange HigherOrEqualThan(Version version)
    {
        return new VersionRange(version, null, true, false);
    }

    public static VersionRange HigherOrEqualThan(string version)
    {
        return HigherOrEqualThan(Version.Parse(version));
    }

    public static VersionRange LowerThan(Version version)
    {
        return new VersionRange(null, version, false, false);
    }

    public static VersionRange LowerThan(string version)
    {
        return LowerThan(Version.Parse(version));
    }

    public static VersionRange LowerOrEqualThan(Version version)
    {
        return new VersionRange(null, version, true, false);
    }

    public static VersionRange LowerOrEqualThan(string version)
    {
        return LowerOrEqualThan(Version.Parse(version));
    }

    public Version? LowerRange { get; }
        
    public Version? UpperRange { get; }
        
    public bool UpperVersionIncluded { get; }

    public bool LowerVersionIncluded { get; }

    public static int CustomCompareVersions(Version? first, Version? second)
    {
        if (second == null)
        {
            return first == null ? 0 : -1;
        }

        if (first == null)
        {
            return 1;
        }

        var compare = first.CompareTo(second);
        if (compare == 0)
        {
            return 0;
        }

        var firstVersion = first.ToString().Split('.');
        var secondVersion = second.ToString().Split('.');

        if (firstVersion.Length == secondVersion.Length)
        {
            return compare;
        }

        var appendExtraZeroesFirst = 4 - Math.Min(4, firstVersion.Length);
        var appendExtraZeroesSecond = 4 - Math.Min(4, secondVersion.Length);
        
        var newFirstVersion = Version.Parse(string.Join('.', firstVersion.Concat(Enumerable.Repeat("0", appendExtraZeroesFirst))));
        var newSecondVersion = Version.Parse(string.Join('.', secondVersion.Concat(Enumerable.Repeat("0", appendExtraZeroesSecond))));

        return newFirstVersion.CompareTo(newSecondVersion);
    }

    /// <summary>
    /// Compares both VersionRange instances.
    /// </summary>
    /// <param name="other">The other instance to compare.</param>
    /// <returns>Returns -1 if this instance is semantically "lower" than the other one, 0 if both are considered equal, and 1 otherwise.</returns>
    public int CompareTo(VersionRange? other)
    {
        if (other == null)
        {
            return -1;
        }

        if (this.LowerRange == null && other.LowerRange != null)
        {
            return -1;
        }

        if (this.LowerRange != null && other.LowerRange == null)
        {
            return 1;
        }

        if (this.LowerRange != null && other.LowerRange != null)
        {
            var compareLower = CustomCompareVersions(this.LowerRange, other.LowerRange);

            switch (compareLower)
            {
                case 0:
                    if (other.LowerVersionIncluded && !this.LowerVersionIncluded)
                    {
                        return 1;
                    }

                    if (!other.LowerVersionIncluded && this.LowerVersionIncluded)
                    {
                        return -1;
                    }

                    break;
                default:
                    return compareLower;
            }
        }

        if (this.UpperRange == null && other.UpperRange != null)
        {
            return -1;
        }

        if (this.UpperRange != null && other.UpperRange == null)
        {
            return 1;
        }

        if (this.UpperRange != null && other.UpperRange != null)
        {
            var compareUpper = CustomCompareVersions(this.UpperRange, other.UpperRange);

            switch (compareUpper)
            {
                case 0:
                    if (other.UpperVersionIncluded && !this.UpperVersionIncluded)
                    {
                        return -1;
                    }

                    if (!other.UpperVersionIncluded && this.UpperVersionIncluded)
                    {
                        return 1;
                    }

                    break;
                default:
                    return compareUpper;
            }
        }

        return 0;
    }

    public bool Matches(string version)
    {
        return this.Matches(Version.Parse(version));
    }

    public bool Matches(Version version)
    {
        if (this.LowerRange != null)
        {
            var compareLower = CustomCompareVersions(version, this.LowerRange);
            switch (compareLower)
            {
                case 0:
                    if (!this.LowerVersionIncluded)
                    {
                        return false;
                    }

                    break;
                case -1:
                    return false;
            }
        }

        if (this.UpperRange != null)
        {
            var compareUpper = CustomCompareVersions(version, this.UpperRange);
            switch (compareUpper)
            {
                case 0:
                    if (!this.UpperVersionIncluded)
                    {
                        return false;
                    }

                    break;
                case 1:
                    return false;
            }
        }

        return true;
    }
}