using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Commands.Dashboard
{
    public class SetToolFilterCommand : UiCommand
    {
        public SetToolFilterCommand(string searchKey)
        {
            this.SearchKey = searchKey;
        }
        
        public string SearchKey { get; set; }
    }
}
