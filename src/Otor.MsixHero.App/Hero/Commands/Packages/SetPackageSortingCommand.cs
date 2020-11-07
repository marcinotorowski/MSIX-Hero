using System;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    [Serializable]
    public class SetPackageSortingCommand : UiCommand
    {
        public SetPackageSortingCommand()
        {
        }

        public SetPackageSortingCommand(PackageSort sortMode, bool? descending = null)
        {
            SortMode = sortMode;
            Descending = descending;
        }

        public PackageSort SortMode { get; set; }

        public bool? Descending { get; set;  }
    }
}
