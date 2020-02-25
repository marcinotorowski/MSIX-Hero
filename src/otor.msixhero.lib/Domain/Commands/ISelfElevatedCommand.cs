namespace otor.msixhero.lib.Domain.Commands
{
    public interface ISelfElevatedCommand
    {
        SelfElevationType RequiresElevation { get; }
    }
}