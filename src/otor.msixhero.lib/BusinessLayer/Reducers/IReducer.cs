using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal interface IReducer
    {
        Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }

    internal interface IReducer<T> : IReducer
    {
        Task<T> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }
}