namespace otor.msixhero.lib.Domain.Commands
{
    public interface ISelfElevatedCommand
    {
        bool RequiresElevation { get; }
    }
}