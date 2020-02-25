using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class SelfElevationReducer : BaseReducer
    {
        protected readonly IElevatedClient ElevatedClient;
        private readonly SelfElevatedCommand selfElevatedCommand;

        protected SelfElevationReducer(
            SelfElevatedCommand selfElevatedCommand, 
            IElevatedClient elevatedClient, 
            IWritableApplicationStateManager stateManager) : base(selfElevatedCommand, stateManager)
        {
            this.ElevatedClient = elevatedClient;
            this.selfElevatedCommand = selfElevatedCommand;
        }

        public sealed override async Task Reduce(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IBusyManager busyManager = default)
        {
            if (busyManager == null)
            {
                switch (this.selfElevatedCommand.RequiresElevation)
                {
                    case SelfElevationType.AsInvoker:
                        await this.ReduceAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        break;

                    case SelfElevationType.HighestAvailable:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            await this.ReduceAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }
                        else if (this.StateManager.CurrentState.IsSelfElevated)
                        {
                            await this.ElevatedClient.Execute(this.selfElevatedCommand, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await this.ReduceAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }

                        break;

                    case SelfElevationType.RequireAdministrator:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            await this.ReduceAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await this.ElevatedClient.Execute(this.selfElevatedCommand, cancellationToken).ConfigureAwait(false);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var context = busyManager.Begin();

                try
                {
                    switch (this.selfElevatedCommand.RequiresElevation)
                    {
                        case SelfElevationType.AsInvoker:
                            await this.ReduceAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                            break;

                        case SelfElevationType.HighestAvailable:
                            if (this.StateManager.CurrentState.IsElevated)
                            {
                                await this.ReduceAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                            }
                            else if (this.StateManager.CurrentState.IsSelfElevated)
                            {
                                await this.ElevatedClient.Execute(this.selfElevatedCommand, cancellationToken, context).ConfigureAwait(false);
                            }
                            else
                            {
                                await this.ReduceAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                            }

                            break;

                        case SelfElevationType.RequireAdministrator:
                            if (this.StateManager.CurrentState.IsElevated)
                            {
                                await this.ReduceAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                            }
                            else
                            {
                                await this.ElevatedClient.Execute(this.selfElevatedCommand, cancellationToken, context).ConfigureAwait(false);
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                finally
                {
                    busyManager.End(context);
                }
            }
        }

        protected abstract Task ReduceAsCurrentUser(
            IInteractionService interactionService, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default);
    }

    public abstract class SelfElevationReducer<T> : BaseReducer<T>
    {
        protected readonly IElevatedClient ElevatedClient;
        private readonly SelfElevatedCommand<T> selfElevatedCommand;

        protected SelfElevationReducer(SelfElevatedCommand<T> selfElevatedCommand, IElevatedClient elevatedClient, IWritableApplicationStateManager stateManager) : base(selfElevatedCommand, stateManager)
        {
            this.ElevatedClient = elevatedClient;
            this.selfElevatedCommand = selfElevatedCommand;
        }

        public sealed override async Task<T> GetReduced(IInteractionService interactionService, CancellationToken cancellationToken = default, IBusyManager busyManager = default)
        {
            if (busyManager == null)
            {
                switch (this.selfElevatedCommand.RequiresElevation)
                {
                    case SelfElevationType.AsInvoker:
                        return await this.GetReducedAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);

                    case SelfElevationType.HighestAvailable:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }
                        else if (this.StateManager.CurrentState.IsSelfElevated)
                        {
                            return await this.ElevatedClient.GetExecuted(this.selfElevatedCommand, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }

                    case SelfElevationType.RequireAdministrator:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            return await this.ElevatedClient.GetExecuted(this.selfElevatedCommand, cancellationToken).ConfigureAwait(false);
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var context = busyManager.Begin();
            try
            {
                switch (this.selfElevatedCommand.RequiresElevation)
                {
                    case SelfElevationType.AsInvoker:
                        return await this.GetReducedAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                            
                    case SelfElevationType.HighestAvailable:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                        }
                        else if (this.StateManager.CurrentState.IsSelfElevated)
                        {
                            return await this.ElevatedClient.GetExecuted(this.selfElevatedCommand, cancellationToken, context).ConfigureAwait(false);
                        }
                        else
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                        }
                            
                    case SelfElevationType.RequireAdministrator:
                        if (this.StateManager.CurrentState.IsElevated)
                        {
                            return await this.GetReducedAsCurrentUser(interactionService, cancellationToken, context).ConfigureAwait(false);
                        }
                        else
                        {
                            return await this.ElevatedClient.GetExecuted(this.selfElevatedCommand, cancellationToken, context).ConfigureAwait(false);
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            finally
            {
                busyManager.End(context);
            }
        }

        protected abstract Task<T> GetReducedAsCurrentUser(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default);
    }
}