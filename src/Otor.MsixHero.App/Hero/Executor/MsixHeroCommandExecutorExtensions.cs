// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Mvvm.Progress;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Executor
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
            private static readonly LogSource Logger = new();
            private readonly IMsixHeroCommandExecutor commandExecutor;

            private IInteractionService interactionService;

            private bool allowRetry;

            private IBusyManager busyManager;

            private OperationType operationType;

            private MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor commandExecutor, IBusyManager manager, OperationType operation) : this(commandExecutor)
            {
                this.busyManager = manager;
                this.operationType = operation;
            }

            private MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor commandExecutor, IInteractionService interaction, bool allowUserRetry) : this(commandExecutor)
            {
                this.allowRetry = allowUserRetry;
                this.interactionService = interaction;
            }

            private MsixHeroDecoratedCommandExecutor(IMsixHeroCommandExecutor commandExecutor)
            {
                this.commandExecutor = commandExecutor;
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
                get => this.commandExecutor.ApplicationState;
                set => this.commandExecutor.ApplicationState = value;
            }

            public async Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : IRequest
            {
                IBusyContext busyContext = null;

                if (this.busyManager != null)
                {
                    busyContext = this.busyManager.Begin(this.operationType);
                }

                IProgress<ProgressData> innerProgress = new ProxyProgressReporter(progress, busyContext);

                try
                {
                    await this.commandExecutor.Invoke(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                }
                catch (OperationCanceledException e)
                {
                    Logger.Warn().WriteLine(e.Message, e);
                }
                catch (Exception e)
                {
                    Logger.Error().WriteLine(e.Message, e);
                    if (this.interactionService != null)
                    {
                        if (this.ShowError(e) == InteractionResult.Retry)
                        {
                            Logger.Info().WriteLine("The user wanted to retry…");
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

            public async Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : IRequest<TResult>
            {
                IBusyContext busyContext = null;

                if (this.busyManager != null)
                {
                    busyContext = this.busyManager.Begin(this.operationType);
                }

                IProgress<ProgressData> innerProgress = new ProxyProgressReporter(progress, busyContext);

                try
                {
                    return await this.commandExecutor.Invoke<TCommand, TResult>(sender, command, cancellationToken, innerProgress).ConfigureAwait(false);
                }
                catch (OperationCanceledException e)
                {
                    Logger.Warn().WriteLine(e);
                    return default;
                }
                catch (Exception e)
                {
                    Logger.Error().WriteLine(e);
                    if (this.interactionService != null)
                    {
                        if (this.ShowError(e) == InteractionResult.Retry)
                        {
                            Logger.Info().WriteLine("The user wanted to retry…");
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
                    var message = win32Exception.NativeErrorCode == 1223 ? Resources.Localization.UacApp_AdminRightsRequired : exception.Message;
                    return this.interactionService.ShowError(message, win32Exception, buttons: buttons);
                }

                if (exception is UnauthorizedAccessException)
                {
                    return this.interactionService.ShowError(Resources.Localization.UacApp_AdminRightsRequired, exception, buttons: buttons);
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