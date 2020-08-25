using System;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Packages
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
