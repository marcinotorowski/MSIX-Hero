using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.App.Hero.State;

namespace Otor.MsixHero.App.Hero.Commands
{
    public class SetCurrentModeCommand : UiCommand
    {
        public SetCurrentModeCommand(ApplicationMode newMode)
        {
            this.NewMode = newMode;
        }

        public ApplicationMode NewMode { get; private set; }
    }
}
