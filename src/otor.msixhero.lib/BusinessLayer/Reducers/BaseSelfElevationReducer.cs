using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseSelfElevationReducer<T> : BaseReducer<T> where T : IApplicationState
    {
        protected readonly IProcessManager processManager;

        protected BaseSelfElevationReducer(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        protected async Task SelfElevateAndExecute<TInput>(TInput inputAction, CancellationToken cancellationToken) where TInput : BaseCommand
        {
            var client = new Client(processManager);
            await client.Execute<TInput>(inputAction, cancellationToken);
        }
    }
}