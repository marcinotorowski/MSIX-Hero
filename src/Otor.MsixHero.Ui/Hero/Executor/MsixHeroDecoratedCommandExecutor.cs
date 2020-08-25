using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero.Commands.Base;
using Otor.MsixHero.Ui.Hero.State;

namespace Otor.MsixHero.Ui.Hero.Executor
{

    public static class MsixHeroCommandExecutorExtensions
    {
        public static IMsixHeroCommandExecutor WithBusyManager(this IMsixHeroCommandExecutor executor, IBusyManager busyManager, OperationType operationType)
        {
            return MsixHeroDecoratedCommandExecutor.From(executor, busyManager, operationType);
        }

        public static IMsixHeroCommandExecutor WithErrorHandling(this IMsixHeroCommandExecutor executor, IInteractionService interactionService, bool allowRetry)
        {
            return MsixHeroDecoratedCommandExecutor.From(executor, interactionService, allowRetry);
        }

        private class MsixHeroDecoratedCommandExecutor : IMsixHeroCommandExecutor
        {
            private readonly IMsixHeroCommandExecutor decorated;

            private IInteractionService interactionService;

            private bool allowRetry;

            private IBusyManager busyManager;

            private OperationType operationType;

            public MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor decorated, IBusyManager manager, OperationType operation) : this(decorated)
            {
                this.busyManager = manager;
                this.operationType = operation;
            }

            public MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor decorated, IInteractionService interaction, bool allowUserRetry) : this(decorated)
            {
                this.allowRetry = allowUserRetry;
                this.interactionService = interaction;
            }

            private MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor decorated)
            {
                this.decorated = decorated;
            }

            public static MsixHeroDecoratedCommandExecutor From(IMsixHeroCommandExecutor executor, IBusyManager busyManager, OperationType operationType)
            {
                var result = executor as MsixHeroDecoratedCommandExecutor;

                if (result == null)
                {
                    result = new MsixHeroDecoratedCommandExecutor(executor, busyManager, operationType);
                }
                else
                {
                    result.busyManager = busyManager;
                    result.operationType = operationType;
                }

                return result;
            }

            public static MsixHeroDecoratedCommandExecutor From(IMsixHeroCommandExecutor executor, IInteractionService interactionService, bool allowRetry)
            {
                var result = executor as MsixHeroDecoratedCommandExecutor;

                if (result == null)
                {
                    result = new MsixHeroDecoratedCommandExecutor(executor, interactionService, allowRetry);
                }
                else
                {
                    result.interactionService = interactionService;
                    result.allowRetry = allowRetry;
                }

                return result;
            }

            public MsixHeroState ApplicationState
            {
                get => this.decorated.ApplicationState;
                set => this.decorated.ApplicationState = value;
            }

            public async Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand
            {
                IBusyContext busyContext = null;

                if (this.busyManager != null)
                {
                    busyContext = this.busyManager.Begin(this.operationType);
                }

                IProgress<ProgressData> innerProgress = new ProxyProgressReporter(progress, busyContext);

                try
                {
                    await this.decorated.Invoke(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (this.interactionService != null)
                    {
                        if (this.ShowError(e) == InteractionResult.Retry)
                        {
                            await this.Invoke(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    if (busyContext != null)
                    {
                        this.busyManager.End(busyContext);
                    }
                }
            }

            public async Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand<TResult>
            {
                IBusyContext busyContext = null;

                if (this.busyManager != null)
                {
                    busyContext = this.busyManager.Begin(this.operationType);
                }

                IProgress<ProgressData> innerProgress = new ProxyProgressReporter(progress, busyContext);

                try
                {
                    return await this.decorated.Invoke<TCommand, TResult>(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (this.interactionService != null)
                    {
                        if (this.ShowError(e) == InteractionResult.Retry)
                        {
                            return await this.Invoke<TCommand, TResult>(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                        }
                    }

                    return default;
                }
                finally
                {
                    if (busyContext != null)
                    {
                        this.busyManager.End(busyContext);
                    }
                }
            }

            private InteractionResult ShowError(Exception exception)
            {
                var buttons = InteractionResult.Close;
                if (this.allowRetry)
                {
                    buttons |= InteractionResult.Retry;
                }

                if (exception is Win32Exception win32Exception)
                {
                    var message = win32Exception.NativeErrorCode == 1223 ? "This operation requires administrative rights." : exception.Message;
                    return this.interactionService.ShowError(message, win32Exception, buttons: buttons);
                }

                if (exception is UnauthorizedAccessException)
                {
                    var message = "This operation requires administrative rights.";
                    return this.interactionService.ShowError(message, exception, buttons: buttons);
                }

                return this.interactionService.ShowError(exception.Message, exception, buttons: buttons);
            }

            private class ProxyProgressReporter : IProgress<ProgressData>
            {
                private readonly IProgress<ProgressData> decorated;
                private readonly IBusyContext busyContext;

                public ProxyProgressReporter(IProgress<ProgressData> decorated = null, IBusyContext busyContext = null)
                {
                    this.decorated = decorated;
                    this.busyContext = busyContext;
                }

                public void Report(ProgressData value)
                {
                    this.busyContext?.Report(value);
                    this.decorated?.Report(value);
                }
            }
        }
    }
}