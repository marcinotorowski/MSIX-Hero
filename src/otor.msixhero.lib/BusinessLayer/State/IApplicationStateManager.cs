using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Commanding;
using otor.msixhero.lib.Infrastructure.Ipc;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IApplicationStateManager
    {
        IApplicationState CurrentState { get; }

        ICommandExecutor CommandExecutor { get; }

        IEventAggregator EventAggregator { get; }
    }
}
