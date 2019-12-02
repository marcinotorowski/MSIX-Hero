using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Reducers;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Commands.Manager;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Domain.Commands.UI;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public class CommandExecutor : ICommandExecutor
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly IDictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>> reducerFactories = new Dictionary<Type, Func<BaseCommand, IReducer<ApplicationState>>>();
        private readonly ApplicationStateManager stateManager;
        private readonly IAppxPackageManagerFactory appxPackageManagerFactory;
        private readonly IInteractionService interactionService;
        private readonly IBusyManager busyManager;

        public CommandExecutor(
            ApplicationStateManager stateManager,
            IAppxPackageManagerFactory appxPackageManagerFactory,
            IInteractionService interactionService,
            IBusyManager busyManager)
        {
            this.stateManager = stateManager;
            this.appxPackageManagerFactory = appxPackageManagerFactory;
            this.interactionService = interactionService;
            this.busyManager = busyManager;

            this.ConfigureReducers();
        }

        public void Execute(BaseCommand action)
        {
            try
            {
                this.ExecuteAsync(action, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public T GetExecute<T>(BaseCommand<T> action)
        {
            try
            {
                return this.GetExecuteAsync(action, CancellationToken.None).Result;
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public async Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
                {
                    return;
                }

                var lazyReducer = reducerFactory(action);


                bool elevate;
                if (action is ISelfElevatedCommand selfElevateCommand)
                {
                    elevate = selfElevateCommand.RequiresElevation;

                    if (this.stateManager.CurrentState.IsElevated)
                    {
                        elevate = false;
                    }
                }
                else
                {
                    elevate = false;
                }

                var packageManager = elevate ? this.appxPackageManagerFactory.GetRemote() : this.appxPackageManagerFactory.GetLocal();
                await lazyReducer.Reduce(this.interactionService, packageManager, cancellationToken).ConfigureAwait(false);

                if (elevate && !this.stateManager.CurrentState.IsSelfElevated)
                {
                    this.stateManager.CurrentState.IsSelfElevated = true;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("Operation cancelled by the user.");
                throw;
            }
            catch (Win32Exception e)
            {
                Logger.Error(e, "Win32 error during command execution.");
                // If error code is 1223 it means that the user did not press YES in UAC.
                var message = e.NativeErrorCode == 1223 ? "This operation requires administrative rights." : e.Message;
                var result = this.interactionService.ShowError(message, extendedInfo: e.ToString());
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying...");
                    await this.ExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "General error during command execution.");
                var result = this.interactionService.ShowError(e.Message, extendedInfo: e.ToString());
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying...");
                    await this.ExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<T> GetExecuteAsync<T>(BaseCommand<T> action, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
                {
                    return default;
                }

                var lazyReducer = reducerFactory(action);
                var lazyReducerOutput = lazyReducer as IReducer<ApplicationState, T>;
                if (lazyReducerOutput == null)
                {
                    throw new NotSupportedException("This reducer does not support output.");
                }

                bool elevate;

                if (action is ISelfElevatedCommand selfElevateCommand)
                {
                    elevate = selfElevateCommand.RequiresElevation;

                    if (this.stateManager.CurrentState.IsElevated)
                    {
                        elevate = false;
                    }
                }
                else
                {
                    elevate = false;
                }

                var packageManager = elevate
                    ? this.appxPackageManagerFactory.GetRemote()
                    : this.appxPackageManagerFactory.GetLocal();

                var result = await lazyReducerOutput.GetReduced(this.interactionService, packageManager, cancellationToken).ConfigureAwait(false);

                if (elevate && !this.stateManager.CurrentState.IsSelfElevated)
                {
                    this.stateManager.CurrentState.IsSelfElevated = true;
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                Logger.Warn("Operation cancelled by the user.");
                throw;
            }
            catch (Win32Exception e)
            {
                Logger.Error(e, "Win32 error during command execution.");
                // If error code is 1223 it means that the user did not press YES in UAC.
                var message = e.NativeErrorCode == 1223 ? "This operation requires administrative rights." : e.Message;
                var result = this.interactionService.ShowError(message, extendedInfo: e.ToString());

                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generic error during command execution.");
                var result = this.interactionService.ShowError(e.Message, extendedInfo: e.ToString());
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
        }

        private void ConfigureReducers()
        {
            this.reducerFactories[typeof(SetPackageFilter)] = action => new SetPackageFilterReducer((SetPackageFilter)action, this.stateManager);
            this.reducerFactories[typeof(SetPackageContext)] = action => new SetPackageContextReducer((SetPackageContext)action, this.stateManager);
            this.reducerFactories[typeof(GetPackages)] = action => new GetPackagesReducer((GetPackages)action, this.stateManager, this.busyManager);
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action, this.stateManager);
            this.reducerFactories[typeof(GetRegistryMountState)] = action => new GetRegistryMountStateReducer((GetRegistryMountState)action, this.stateManager);
            this.reducerFactories[typeof(RunPackage)] = action => new RunPackageReducer((RunPackage)action, this.stateManager);
            this.reducerFactories[typeof(RunToolInPackage)] = action => new RunToolInPackageReducer((RunToolInPackage)action, this.stateManager);
            this.reducerFactories[typeof(RemovePackages)] = action => new RemovePackageReducer((RemovePackages)action, this.stateManager, this.busyManager);
            this.reducerFactories[typeof(FindUsers)] = action => new FindUsersReducer((FindUsers)action, this.stateManager);
            this.reducerFactories[typeof(GetUsersOfPackage)] = action => new GetUsersOfPackageReducer((GetUsersOfPackage)action, this.stateManager);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action, this.stateManager);
            this.reducerFactories[typeof(MountRegistry)] = action => new MountRegistryReducer((MountRegistry)action, this.stateManager);
            this.reducerFactories[typeof(UnmountRegistry)] = action => new UnmountRegistryReducer((UnmountRegistry)action, this.stateManager, this.busyManager);
            this.reducerFactories[typeof(SetPackageSorting)] = action => new SetPackageSortingReducer((SetPackageSorting)action, this.stateManager);
            this.reducerFactories[typeof(SetPackageGrouping)] = action => new SetPackageGroupingReducer((SetPackageGrouping)action, this.stateManager);
            this.reducerFactories[typeof(GetLogs)] = action => new GetLogsReducer((GetLogs)action, this.stateManager);
            this.reducerFactories[typeof(AddPackage)] = action => new AddPackageReducer((AddPackage)action, this.stateManager, this.busyManager);
            this.reducerFactories[typeof(GetPackageDetails)] = action => new GetPackageDetailsReducer((GetPackageDetails)action, this.stateManager);
            this.reducerFactories[typeof(InstallCertificate)] = action => new InstallCertificateReducer((InstallCertificate)action, this.stateManager, this.busyManager);
        }
    }
}
