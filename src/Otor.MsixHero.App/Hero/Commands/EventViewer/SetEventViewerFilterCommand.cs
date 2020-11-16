using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Commands.EventViewer
{
    public class SetEventViewerFilterCommand : UiCommand
    {
        public SetEventViewerFilterCommand(string searchKey)
        {
            this.SearchKey = searchKey;
        }
        
        public string SearchKey { get; set; }
    }
}
