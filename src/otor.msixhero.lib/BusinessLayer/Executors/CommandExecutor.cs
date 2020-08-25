using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Domain.Commands;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors
{
    public abstract class CommandExecutor : ICommandExecutor
    {
        protected CommandExecutor(
            // ReSharper disable once UnusedParameter.Local
            ProxyObject command)
        {
        }

        public abstract Task Execute(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default);
    }
}
