using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors
{
    public abstract class CommandWithOutputExecutor<T> : CommandExecutor, ICommandWithOutputExecutor<T>
    {
        protected CommandWithOutputExecutor(
            ProxyObject<T> command) : base(command)
        {
        }
        
        public abstract Task<T> ExecuteAndReturn(IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);

        public override Task Execute(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default)
        {
            return this.ExecuteAndReturn(interactionService, cancellationToken, progressData);
        }
    }
}