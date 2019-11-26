using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Infrastructure;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Managers;
using otor.msixhero.lib.Services;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    internal interface IReducer<in T> where T : IApplicationState
    {
        Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }

    internal interface IReducer<in T, TOutput> : IReducer<T> where T : IApplicationState
    {
        Task<TOutput> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);
    }
}