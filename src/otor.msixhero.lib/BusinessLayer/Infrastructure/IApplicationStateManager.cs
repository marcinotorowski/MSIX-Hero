using otor.msixhero.lib.BusinessLayer.State;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface IApplicationStateManager
    {
        IApplicationState CurrentState { get; }

        IActionExecutor Executor { get; }

        IEventAggregator EventAggregator { get; }
    }

    public interface IApplicationStateManager<out T> : IApplicationStateManager where T : IApplicationState
    {
        new T CurrentState { get; }
    }
}
