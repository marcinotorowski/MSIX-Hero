using System;
using otor.msixhero.lib.Domain.Appx.Packages;

namespace otor.msixhero.lib.Domain.Commands.Grid
{
    [Serializable]
    public class SetPackageSorting : BaseCommand<PackageSort>
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
