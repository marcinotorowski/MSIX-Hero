using System.Threading.Tasks;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Commanding;
using Prism.Events;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IApplicationStateManager
    {
        IApplicationState CurrentState { get; }

        ICommandExecutor CommandExecutor { get; }

        IEventAggregator EventAggregator { get; }
    }

    public interface IApplicationStateManager<out T> : IApplicationStateManager where T : IApplicationState
    {
        new T CurrentState { get; }
    }
}
