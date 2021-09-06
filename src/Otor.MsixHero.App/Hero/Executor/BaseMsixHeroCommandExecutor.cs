// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.Infrastructure.Progress;
using Prism.Events;

namespace Otor.MsixHero.App.Hero.Executor
{
    public abstract class BaseMsixHeroCommandExecutor : IMsixHeroCommandExecutor
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IMediator mediator;

        protected BaseMsixHeroCommandExecutor(IEventAggregator eventAggregator, IMediator mediator)
        {
            this.eventAggregator = eventAggregator;
            this.mediator = mediator;
        }

        public MsixHeroState ApplicationState { get; set; }

        private bool IsCommandWithOutput(Type commandType, out Type resultType)
        {
            if (commandType.IsGenericType && commandType.GetGenericArguments().Length == 1)
            {
                if (commandType.GetGenericTypeDefinition() == typeof(IRequest<>))
                {
                    resultType = commandType.GetGenericArguments()[0];
                    return true;
                }

                if (typeof(IRequest<>).IsAssignableFrom(commandType.GetGenericTypeDefinition()))
                {
                    resultType = commandType.GetGenericArguments()[0];
                    return true;
                }
            }

            if (commandType.BaseType == null || commandType.BaseType == typeof(object))
            {
                resultType = null;
                return false;
            }

            return this.IsCommandWithOutput(commandType.BaseType, out resultType);
        }

        public async Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : IRequest
        {
            if (this.IsCommandWithOutput(command.GetType(), out var resultType))
            {
                // this will call the overload that returns the results
                var findMethod = typeof(BaseMsixHeroCommandExecutor).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(m => m.Name == nameof(Invoke) && m.ReturnType.IsGenericType);

                findMethod = findMethod.MakeGenericMethod(typeof(TCommand), resultType);
                var task = (Task)findMethod.Invoke(this, new[] { sender,  command,  cancellationToken,  progress });

                if (task == null)
                {
                    throw new InvalidOperationException();
                }

                await task.ConfigureAwait(false);
                return;
            }
            
            var payload = new UiExecutingPayload<TCommand>(sender, command);
            this.eventAggregator.GetEvent<UiExecutingEvent<TCommand>>().Publish(payload);
            if (payload.Cancel)
            {
                this.eventAggregator.GetEvent<UiCancelledEvent<TCommand>>().Publish(new UiCancelledPayload<TCommand>(sender, command));
            }

            try
            {
                this.eventAggregator.GetEvent<UiStartedEvent<TCommand>>().Publish(new UiStartedPayload<TCommand>(sender, command));
                var unit = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);
                this.eventAggregator.GetEvent<UiExecutedEvent<TCommand>>().Publish(new UiExecutedPayload<TCommand>(sender, command));
            }
            catch (OperationCanceledException)
            {
                this.eventAggregator.GetEvent<UiCancelledEvent<TCommand>>().Publish(new UiCancelledPayload<TCommand>(sender, command));
                throw;
            }
            catch (Exception exception)
            {
                this.eventAggregator.GetEvent<UiFailedEvent<TCommand>>().Publish(new UiFailedPayload<TCommand>(sender, command, exception));
                throw;
            }
        }

        public async Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : IRequest<TResult>
        {
            var payload = new UiExecutingPayload<TCommand>(sender, command);
            this.eventAggregator.GetEvent<UiExecutingEvent<TCommand>>().Publish(payload);
            if (payload.Cancel)
            {
                this.eventAggregator.GetEvent<UiCancelledEvent<TCommand>>().Publish(new UiCancelledPayload<TCommand>(sender, command));
            }

            try
            {
                this.eventAggregator.GetEvent<UiStartedEvent<TCommand>>().Publish(new UiStartedPayload<TCommand>(sender, command));

                var result = await this.mediator.Send(command, cancellationToken).ConfigureAwait(false);

                this.eventAggregator.GetEvent<UiExecutedEvent<TCommand, TResult>>().Publish(new UiExecutedPayload<TCommand, TResult>(sender, command, result));
                this.eventAggregator.GetEvent<UiExecutedEvent<TCommand>>().Publish(new UiExecutedPayload<TCommand>(sender, command));
                return result;
            }
            catch (OperationCanceledException)
            {
                this.eventAggregator.GetEvent<UiCancelledEvent<TCommand>>().Publish(new UiCancelledPayload<TCommand>(sender, command));
                throw;
            }
            catch (Exception exception)
            {
                this.eventAggregator.GetEvent<UiFailedEvent<TCommand>>().Publish(new UiFailedPayload<TCommand>(sender, command, exception));
                throw;
            }
        }
    }
}