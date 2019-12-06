using otor.msixhero.lib.Domain.State;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IWritableApplicationStateManager : IApplicationStateManager
    {
        new IWritableApplicationState CurrentState { get; }
    }
}