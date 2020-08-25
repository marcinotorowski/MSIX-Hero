using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands.Volumes
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
