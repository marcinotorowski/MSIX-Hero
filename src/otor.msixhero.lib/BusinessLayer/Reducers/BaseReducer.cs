using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseReducer<T> : IReducer<T> where T : IApplicationState
    {
        public abstract Task<bool> ReduceAsync(IApplicationStateManager<T> state, CancellationToken cancellationToken);

        protected async Task<TOutput> SelfElevateAndExecute<TInput, TOutput>(TInput inputAction, CancellationToken cancellationToken) where TInput : BaseAction
        {
            var client = new Client();
            return await client.Execute<TInput, TOutput>(inputAction, cancellationToken);
        }
    }

    public class RemoteInputOutput<TInput, TOutput> where TInput : BaseAction
    {
        public RemoteInputOutput()
        {
        }

        public RemoteInputOutput(TInput input, TOutput output)
        {
            Input = input;
            Output = output;
        }

        public TInput Input { get; set; }

        public TOutput Output { get; set; }
    }
}
