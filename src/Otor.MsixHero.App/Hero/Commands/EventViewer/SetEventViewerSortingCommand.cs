using System;
using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.EventViewer
{
    [Serializable]
    public class SetEventViewerSortingCommand : UiCommand
    {
        public SetEventViewerSortingCommand()
        {
        }

        public SetEventViewerSortingCommand(EventSort sortMode, bool? descending = null)
        {
            SortMode = sortMode;
            Descending = descending;
        }

        public EventSort SortMode { get; set; }

        public bool? Descending { get; set;  }
    }
}
