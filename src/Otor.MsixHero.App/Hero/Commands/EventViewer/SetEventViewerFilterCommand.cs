using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.EventViewer
{
    public class SetEventViewerFilterCommand : UiCommand
    {
        public SetEventViewerFilterCommand(
            EventFilter filter, 
            string searchKey)
        {
            this.Filter = filter;
            this.SearchKey = searchKey;
        }

        public EventFilter Filter { get; set; }

        public string SearchKey { get; set; }
    }
}
