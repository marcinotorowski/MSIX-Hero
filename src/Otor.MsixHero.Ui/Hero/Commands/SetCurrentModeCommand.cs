using Otor.MsixHero.Lib.Domain.State;
using Otor.MsixHero.Ui.Hero.Commands.Base;
using Otor.MsixHero.Ui.Hero.State;

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
