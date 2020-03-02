using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.BusinessLayer.Appx.AppAttach;
using otor.msixhero.lib.BusinessLayer.Appx.VolumeManager;
using otor.msixhero.lib.BusinessLayer.Reducers;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Commands.Generic;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Domain.Commands.Volumes;
using otor.msixhero.lib.Domain.Exceptions;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public class CommandExecutor : ICommandExecutor
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly IDictionary<Type, Func<BaseCommand, IReducer>> reducerFactories = new Dictionary<Type, Func<BaseCommand, IReducer>>();
        private readonly IAppxPackageManager appxPackageManager;
        private readonly IInteractionService interactionService;
        private readonly IAppAttach appAttach;
        private readonly IElevatedClient elevatedClient;
        private readonly IAppxVolumeManager appxVolumeManager;
        private IWritableApplicationStateManager writableApplicationStateManager;

        public CommandExecutor(
            IAppxPackageManager appxPackageManager,
            IAppxVolumeManager appxVolumeManager,
            IInteractionService interactionService,
            IElevatedClient elevatedClient,
            IAppAttach appAttach)
        {
            this.appxVolumeManager = appxVolumeManager;
            this.appxPackageManager = appxPackageManager;
            this.interactionService = interactionService;
            this.elevatedClient = elevatedClient;
            this.appAttach = appAttach;

            this.ConfigureReducers();
        }

        public void SetStateManager(IWritableApplicationStateManager stateManager)
        {
            this.writableApplicationStateManager = stateManager;
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

        public async Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            try
            {
                if (!this.reducerFactories.TryGetValue(action.GetType(), out var reducerFactory))
                {
                    return;
                }

                var lazyReducer = reducerFactory(action);
                await lazyReducer.Reduce(this.interactionService, cancellationToken, progressReporter).ConfigureAwait(false);

                if (lazyReducer is IFinalizingReducer finalizingReducer)
                {
                    await finalizingReducer.Finish(cancellationToken).ConfigureAwait(false);
                }

                var startedAsAdmin = false;
                if (action is ISelfElevatedCommand selfElevatedCommand)
                {
                    switch (selfElevatedCommand.RequiresElevation)
                    {
                        case SelfElevationType.AsInvoker:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsElevated;
                            break;
                        case SelfElevationType.HighestAvailable:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsSelfElevated;
                            break;
                        case SelfElevationType.RequireAdministrator:
                            startedAsAdmin = true;
                            break;
                    }
                }

                if (startedAsAdmin && !this.writableApplicationStateManager.CurrentState.IsSelfElevated && !this.writableApplicationStateManager.CurrentState.IsElevated)
                {
                    this.writableApplicationStateManager.CurrentState.IsSelfElevated = true;
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
                var result = this.interactionService.ShowError(message, e, buttons: InteractionResult.Close | InteractionResult.Retry);
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying...");
                    await this.ExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (DeveloperModeException e)
            {
                Logger.Warn(e);
                IReadOnlyCollection<string> buttons = new[]
                {
                    $"Go to developer settings{Environment.NewLine}Open Developer Settings page in Modern Control Panel and ensure that side-loading and/or the developer mode is enabled.."
                };

                var result = this.interactionService.ShowMessage(e.Message, buttons);
                if (result == 0)
                {
                    var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
                    Process.Start(process);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error during command execution.");

                var result = this.interactionService.ShowError(e.Message, e, buttons: InteractionResult.Close | InteractionResult.Retry);

                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying...");
                    await this.ExecuteAsync(action, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<object> GetExecuteAsync(BaseCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var resultType = GenericArgumentResolver.GetResultType(command.GetType(), typeof(BaseCommand<>));
            if (resultType == null)
            {
                throw new ArgumentException("The argument does not implement interface BaseCommand<T>.");
            }

            if (!this.reducerFactories.TryGetValue(command.GetType(), out var reducerFactory))
            {
                return default;
            }

            var lazyReducer = reducerFactory(command);
            var reducerResultType = GenericArgumentResolver.GetResultType(lazyReducer.GetType(), typeof(IReducer<>));
            if (reducerResultType == null)
            {
                throw new InvalidOperationException("The reducer does not implement the interface IReducer<,>.");
            }

            if (reducerResultType != resultType)
            {
                throw new InvalidOperationException("Mismatch between generic type of the command and from the reducer.");
            }

            var genericType = typeof(IReducer<>);
            var reducerGenericType = genericType.MakeGenericType(resultType);
            
            try
            {
                Func<object> executor = () =>
                {
                    var methodName = nameof(IReducer<object>.GetReduced);
                    // ReSharper disable once PossibleNullReferenceException
                    try
                    {
                        var invocationResult = reducerGenericType.GetMethod(methodName).Invoke(lazyReducer, new object[] {this.interactionService, cancellationToken, progressReporter });

                        var taskType = typeof(Task<>).MakeGenericType(resultType);

                        // ReSharper disable once PossibleNullReferenceException
                        var returned = taskType.GetProperty(nameof(Task<bool>.Result)).GetValue(invocationResult);
                        return returned;
                    }
                    catch (AggregateException e)
                    {
                        throw e.GetBaseException();
                    }
                };

                bool startedAsAdmin = false;
                object result;
                result = await Task.Run(() => executor(), cancellationToken).ConfigureAwait(false);

                var finalizingGenericType = typeof(IFinalizingReducer<>);
                var reducerFinalizingGenericType = finalizingGenericType.MakeGenericType(resultType);

                if (lazyReducer.GetType().GetInterfaces().Contains(finalizingGenericType))
                {
                    // ReSharper disable once PossibleNullReferenceException
                    var finalizingInvocationResult = reducerFinalizingGenericType.GetMethod(nameof(IFinalizingReducer.Finish)).Invoke(lazyReducer, new []{ result });

                    // ReSharper disable once PossibleNullReferenceException
                    typeof(Task).GetMethod(nameof(Task.Wait)).Invoke(finalizingInvocationResult, new object[0]);
                }

                if (command is ISelfElevatedCommand selfElevatedCommand)
                {
                    switch (selfElevatedCommand.RequiresElevation)
                    {
                        case SelfElevationType.AsInvoker:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsElevated;
                            break;
                        case SelfElevationType.HighestAvailable:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsSelfElevated;
                            break;
                        case SelfElevationType.RequireAdministrator:
                            startedAsAdmin = true;
                            break;
                    }
                }

                if (startedAsAdmin && !this.writableApplicationStateManager.CurrentState.IsSelfElevated && !this.writableApplicationStateManager.CurrentState.IsElevated)
                {
                    this.writableApplicationStateManager.CurrentState.IsSelfElevated = true;
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
                var result = this.interactionService.ShowError(message, e, buttons: InteractionResult.Close | InteractionResult.Retry);

                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generic error during command execution.");
                var result = this.interactionService.ShowError(e.Message, e, buttons: InteractionResult.Close | InteractionResult.Retry);
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
        }

        public async Task<T> GetExecuteAsync<T>(BaseCommand<T> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            try
            {
                if (!this.reducerFactories.TryGetValue(command.GetType(), out var reducerFactory))
                {
                    return default;
                }

                var lazyReducer = reducerFactory(command);
                var lazyReducerOutput = lazyReducer as IReducer<T>;
                if (lazyReducerOutput == null)
                {
                    throw new NotSupportedException("This reducer does not support output.");
                }
                
                T result;
                bool startedAsAdmin = false;
                result = await lazyReducerOutput.GetReduced(this.interactionService, cancellationToken, progressReporter).ConfigureAwait(false);

                if (lazyReducerOutput is IFinalizingReducer<T> finalizingReducer)
                {
                    await finalizingReducer.Finish(result, cancellationToken).ConfigureAwait(false);
                }

                if (command is ISelfElevatedCommand selfElevatedCommand)
                {
                    switch (selfElevatedCommand.RequiresElevation)
                    {
                        case SelfElevationType.AsInvoker:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsElevated;
                            break;
                        case SelfElevationType.HighestAvailable:
                            startedAsAdmin = this.writableApplicationStateManager.CurrentState.IsSelfElevated;
                            break;
                        case SelfElevationType.RequireAdministrator:
                            startedAsAdmin = true;
                            break;
                    }
                }

                if (startedAsAdmin && !this.writableApplicationStateManager.CurrentState.IsSelfElevated && !this.writableApplicationStateManager.CurrentState.IsElevated)
                {
                    this.writableApplicationStateManager.CurrentState.IsSelfElevated = true;
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
                var result = this.interactionService.ShowError(message, e, buttons: InteractionResult.Close | InteractionResult.Retry);

                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generic error during command execution.");
                var result = this.interactionService.ShowError(e.Message, e, buttons: InteractionResult.Close | InteractionResult.Retry);
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
        }

        private void ConfigureReducers()
        {
            this.reducerFactories[typeof(SetPackageFilter)] = action => new SetPackageFilterReducer((SetPackageFilter)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetPackages)] = action => new GetInstalledPackagesReducer((GetPackages)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SelectPackages)] = action => new SelectPackagesReducer((SelectPackages)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetRegistryMountState)] = action => new GetRegistryMountStateReducer((GetRegistryMountState)action, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(RunPackage)] = action => new RunPackageReducer((RunPackage)action, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(RunToolInPackage)] = action => new RunToolInPackageReducer((RunToolInPackage)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(ConvertToVhd)] = action => new ConvertToVhdReducer((ConvertToVhd)action, this.elevatedClient, this.appAttach, this.writableApplicationStateManager);
            this.reducerFactories[typeof(RemovePackages)] = action => new RemovePackageReducer((RemovePackages)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(Deprovision)] = action => new DeprovisionReducer((Deprovision)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(FindUsers)] = action => new FindUsersReducer((FindUsers)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetUsersOfPackage)] = action => new GetUsersOfPackageReducer((GetUsersOfPackage)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SetPackageSidebarVisibility)] = action => new SetPackageSidebarVisibilityReducer((SetPackageSidebarVisibility)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(MountRegistry)] = action => new MountRegistryReducer((MountRegistry)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(UnmountRegistry)] = action => new UnmountRegistryReducer((UnmountRegistry)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SetPackageSorting)] = action => new SetPackageSortingReducer((SetPackageSorting)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SetPackageGrouping)] = action => new SetPackageGroupingReducer((SetPackageGrouping)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetLogs)] = action => new GetLogsReducer((GetLogs)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(AddPackage)] = action => new AddPackageReducer((AddPackage)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetPackageDetails)] = action => new GetPackageDetailsReducer((GetPackageDetails)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(InstallCertificate)] = action => new InstallCertificateReducer((InstallCertificate)action, this.elevatedClient, this.appxPackageManager, this.writableApplicationStateManager);

            this.reducerFactories[typeof(SelectVolumes)] = action => new SelectVolumesReducer((SelectVolumes)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SetMode)] = action => new SetModeReducer((SetMode)action, this.writableApplicationStateManager);
            this.reducerFactories[typeof(AddVolume)] = action => new AddVolumeReducer((AddVolume)action, this.appxVolumeManager, this.elevatedClient, this.writableApplicationStateManager);
            this.reducerFactories[typeof(RemoveVolume)] = action => new RemoveVolumeReducer((RemoveVolume)action, this.appxVolumeManager, this.elevatedClient, this.writableApplicationStateManager);
            this.reducerFactories[typeof(GetVolumes)] = action => new GetVolumesReducer((GetVolumes)action, this.appxVolumeManager, this.writableApplicationStateManager);
            this.reducerFactories[typeof(SetVolumeFilter)] = action => new SetVolumeFilterReducer((SetVolumeFilter)action, this.writableApplicationStateManager);
        }
    }
}
