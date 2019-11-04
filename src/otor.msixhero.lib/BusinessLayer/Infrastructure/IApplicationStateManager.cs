using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface IApplicationStateManager
    {
        IApplicationState CurrentState { get; }

        ICommandExecutor CommandExecutor { get; }

        IEventAggregator EventAggregator { get; }

        Task Initialize();
    }

    public interface IApplicationStateManager<out T> : IApplicationStateManager where T : IApplicationState
    {
        new T CurrentState { get; }
    }
}
