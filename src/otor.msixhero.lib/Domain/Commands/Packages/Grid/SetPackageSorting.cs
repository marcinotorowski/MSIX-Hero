using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Packages.Grid
{
    [Serializable]
    public class SetPackageSorting : CommandWithOutput<PackageSort>
    {
        public SetPackageSorting()
        {
        }

        public SetPackageSorting(PackageSort sortMode, bool? descending = null)
        {
            SortMode = sortMode;
            Descending = descending;
        }

        public PackageSort SortMode { get; set; }

        public bool? Descending { get; set;  }
    }
}
