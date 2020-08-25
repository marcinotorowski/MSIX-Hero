using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.Hero.Commands.Base;

namespace Otor.MsixHero.Ui.Hero.Commands
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
