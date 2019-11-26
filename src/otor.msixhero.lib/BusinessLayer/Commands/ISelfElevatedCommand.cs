namespace otor.msixhero.lib.BusinessLayer.Commands
{
    public interface ISelfElevatedCommand
    {
        bool RequiresElevation { get; }
    }
}