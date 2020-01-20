using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class SelfElevationReducer : BaseReducer
    {
        protected SelfElevationReducer(BaseCommand command, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
        }
    }

    public abstract class SelfElevationReducer<T> : SelfElevationReducer, IReducer<T>
    {
        protected SelfElevationReducer(BaseCommand<T> command, IWritableApplicationStateManager stateManager) : base(command, stateManager)
        {
        }

        public abstract Task<T> GetReduced(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);

        public override Task Reduce(IInteractionService interactionService, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            return this.GetReduced(interactionService, packageManager, cancellationToken);
        }
    }
}