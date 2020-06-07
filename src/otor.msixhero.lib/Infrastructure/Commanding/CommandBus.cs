using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Executors;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.Managers.Registry;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Exceptions;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public abstract class CommandBus : ICommandBus
    {
        protected readonly IDictionary<Type, Func<VoidCommand, ICommandExecutor>> CommandExecutorFactories = new Dictionary<Type, Func<VoidCommand, ICommandExecutor>>();
        protected readonly ISelfElevationManagerFactory<IAppxVolumeManager> VolumeManagerFactory;
        protected readonly ISelfElevationManagerFactory<IAppxPackageManager> PackageManagerFactory;
        protected readonly ISelfElevationManagerFactory<IRegistryManager> RegistryManagerFactory;
        protected readonly ISelfElevationManagerFactory<IAppAttachManager> AppAttachManagerFactory;
        protected readonly ISelfElevationManagerFactory<ISigningManager> SigningManagerFactory;

        protected IWritableApplicationStateManager WritableApplicationStateManager;

        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly IInteractionService interactionService;
        private readonly IElevatedClient elevatedClient;

        protected CommandBus(
            IInteractionService interactionService,
            IElevatedClient elevatedClient,
            ISelfElevationManagerFactory<IAppxVolumeManager> volumeManagerFactory,
            ISelfElevationManagerFactory<IAppxPackageManager> packageManagerFactory,
            ISelfElevationManagerFactory<IRegistryManager> registryManagerFactory,
            ISelfElevationManagerFactory<IAppAttachManager> appAttachManagerFactory,
            ISelfElevationManagerFactory<ISigningManager> signingManagerFactory)
        {
            this.interactionService = interactionService;
            this.elevatedClient = elevatedClient;
            this.VolumeManagerFactory = volumeManagerFactory;
            this.PackageManagerFactory = packageManagerFactory;
            this.RegistryManagerFactory = registryManagerFactory;
            this.AppAttachManagerFactory = appAttachManagerFactory;
            this.SigningManagerFactory = signingManagerFactory;

            // ReSharper disable once VirtualMemberCallInConstructor
            this.RegisterReducers();
        }
        
        public void SetStateManager(IWritableApplicationStateManager stateManager)
        {
            this.WritableApplicationStateManager = stateManager;
        }

        public void Execute(VoidCommand action)
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

        public T GetExecute<T>(CommandWithOutput<T> action)
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

        public async Task ExecuteAsync(VoidCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            try
            {
                if (!this.CommandExecutorFactories.TryGetValue(command.GetType(), out var reducerFactory))
                {
                    throw new InvalidOperationException($"No handler for command {command.GetType().Name} has been registered...");
                }

                var lazyReducer = reducerFactory(command);
                await lazyReducer.Execute(this.interactionService, cancellationToken, progressReporter).ConfigureAwait(false);
                this.WritableApplicationStateManager.CurrentState.IsSelfElevated = await this.elevatedClient.Test(cancellationToken).ConfigureAwait(false);
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
                    await this.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                if (result != InteractionResult.None)
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
            catch (DeveloperModeException e)
            {
                Logger.Warn(e);
                IReadOnlyCollection<string> buttons = new[]
                {
                    $"Go to developer settings{Environment.NewLine}Open Developer Settings page in Modern Control Panel and ensure that side-loading and/or the developer mode is enabled.."
                };

                var result = this.interactionService.ShowMessage(e.Message, buttons, systemButtons: InteractionResult.Close);
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
                    await this.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                    return;
                }
                
                if (result != InteractionResult.None && !(e is UserHandledException))
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
        }

        public async Task<object> GetExecuteAsync(VoidCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            var resultType = GenericArgumentResolver.GetResultType(command.GetType(), typeof(CommandWithOutput<>));
            if (resultType == null)
            {
                throw new ArgumentException("The argument does not implement interface BaseCommand<T>.");
            }

            if (!this.CommandExecutorFactories.TryGetValue(command.GetType(), out var reducerFactory))
            {
                throw new InvalidOperationException($"No handler for command {command.GetType().Name} has been registered...");
            }

            var lazyReducer = reducerFactory(command);
            var reducerResultType = GenericArgumentResolver.GetResultType(lazyReducer.GetType(), typeof(ICommandWithOutputExecutor<>));
            if (reducerResultType == null)
            {
                throw new InvalidOperationException("The reducer does not implement the interface IReducer<,>.");
            }

            if (reducerResultType != resultType)
            {
                throw new InvalidOperationException("Mismatch between generic type of the command and from the reducer.");
            }

            var genericType = typeof(ICommandWithOutputExecutor<>);
            var reducerGenericType = genericType.MakeGenericType(resultType);

            try
            {
                Func<object> executor = () =>
                {
                    var methodName = nameof(ICommandWithOutputExecutor<object>.ExecuteAndReturn);
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

                var result = await Task.Run(() => executor(), cancellationToken).ConfigureAwait(false);
                this.WritableApplicationStateManager.CurrentState.IsSelfElevated = await this.elevatedClient.Test(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException e)
            {
                Logger.Warn("Operation cancelled by the user.");
                throw new UserHandledException(e);
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

                if (result != InteractionResult.None)
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generic error during command execution. " + e.Message);
                var result = this.interactionService.ShowError(e.Message, e, buttons: InteractionResult.Close | InteractionResult.Retry);
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                if (result != InteractionResult.None && !(e is UserHandledException))
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
        }

        public async Task<T> GetExecuteAsync<T>(CommandWithOutput<T> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = default)
        {
            try
            {
                if (!this.CommandExecutorFactories.TryGetValue(command.GetType(), out var reducerFactory))
                {
                    throw new InvalidOperationException($"No handler for command {command.GetType().Name} has been registered...");
                }

                var lazyReducer = reducerFactory(command);
                var lazyReducerOutput = lazyReducer as ICommandWithOutputExecutor<T>;
                if (lazyReducerOutput == null)
                {
                    throw new NotSupportedException("This reducer does not support output.");
                }

                var result = await lazyReducerOutput.ExecuteAndReturn(this.interactionService, cancellationToken, progressReporter).ConfigureAwait(false);
                this.WritableApplicationStateManager.CurrentState.IsSelfElevated = await this.elevatedClient.Test(cancellationToken).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException e)
            {
                Logger.Warn("Operation cancelled by the user.");
                throw new UserHandledException(e);
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

                if (result != InteractionResult.None)
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Generic error during command execution. " + e.Message);
                var result = this.interactionService.ShowError(e.Message, e, buttons: InteractionResult.Close | InteractionResult.Retry);
                if (result == InteractionResult.Retry)
                {
                    Logger.Info("Retrying..");
                    return await this.GetExecuteAsync(command, cancellationToken).ConfigureAwait(false);
                }

                if (result != InteractionResult.None && !(e is UserHandledException))
                {
                    throw new UserHandledException(e);
                }

                throw;
            }
        }

        protected abstract void RegisterReducers();
    }
}