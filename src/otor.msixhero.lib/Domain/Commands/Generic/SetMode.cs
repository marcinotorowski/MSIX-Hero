using otor.msixhero.lib.Domain.State;

namespace otor.msixhero.lib.Domain.Commands.Generic
{
    public class SetMode : BaseCommand
    {
        public SetMode()
        {
        }

        public SetMode(ApplicationMode mode)
        {
            this.Mode = mode;
        }

        public ApplicationMode Mode { get; set; }
    }
}
