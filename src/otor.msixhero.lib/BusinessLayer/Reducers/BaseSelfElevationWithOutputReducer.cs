using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseSelfElevationWithOutputReducer<T, TOutput> : BaseSelfElevationReducer<T>, IReducer<T, TOutput> where T : IApplicationState
    {
        protected BaseSelfElevationWithOutputReducer(IProcessManager processManager) : base(processManager)
        {
        }

        public abstract Task<TOutput> ReduceAndOutputAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);

        protected async Task<TOutput> GetOutputFromSelfElevation<TInput>(TInput inputAction, CancellationToken cancellationToken) where TInput : BaseCommand
        {
            var client = new Client(processManager);
            return await client.Execute<TInput, TOutput>(inputAction, cancellationToken);
        }
    }
}