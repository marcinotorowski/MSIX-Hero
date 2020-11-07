using Otor.MsixHero.App.Hero.Commands.Base;

namespace Otor.MsixHero.App.Hero.Commands.Volumes
{
    public class SetVolumeFilterCommand : UiCommand
    {
        public SetVolumeFilterCommand(string searchKey)
        {
            this.SearchKey = searchKey;
        }
        
        public string SearchKey { get; set; }
    }
}
