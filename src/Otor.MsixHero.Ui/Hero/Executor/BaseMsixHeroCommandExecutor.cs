using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero.Commands.Base;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Hero.State;
using Prism.Events;

namespace Otor.MsixHero.Ui.Hero.Executor
{
    public abstract class BaseMsixHeroCommandExecutor : IMsixHeroCommandExecutor
    {
        protected readonly Dictionary<Type, Func<UiCommand, CancellationToken, IProgress<ProgressData>, Task>> Handlers = new Dictionary<Type, Func<UiCommand, CancellationToken, IProgress<ProgressData>, Task>>();

        private readonly IEventAggregator eventAggregator;

        protected BaseMsixHeroCommandExecutor(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public MsixHeroState ApplicationState { get; set; }

        private bool IsCommandWithOutput(Type commandType, out Type resultType)
        {
            if (commandType.IsGenericType && commandType.GetGenericArguments().Length == 1)
            {
                if (commandType.GetGenericTypeDefinition() == typeof(UiCommand<>))
                {
                    resultType = commandType.GetGenericArguments()[0];
                    return true;
                }

                if (typeof(UiCommand<>).IsAssignableFrom(commandType.GetGenericTypeDefinition()))
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

        public async Task Invoke<TCommand>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand
        {
            if (this.IsCommandWithOutput(command.GetType(), out var resultType))
            {
                var findMethod = typeof(BaseMsixHeroCommandExecutor).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(m => m.Name == nameof(InvokeAndReturn));

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
                await this.Invoke(command, cancellationToken, progress).ConfigureAwait(false);
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

        public Task<TResult> Invoke<TCommand, TResult>(object sender, TCommand command,
            CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
            where TCommand : UiCommand<TResult>
        {
            return this.InvokeAndReturn<TCommand, TResult>(sender, command, cancellationToken, progress);
        }

        private async Task<TResult> InvokeAndReturn<TCommand, TResult>(object sender, TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand<TResult>
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

                var result = await this.Invoke<TCommand, TResult>(command, cancellationToken, progress).ConfigureAwait(false);
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

        protected async Task<TResult> Invoke<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand<TResult>
        {
            if (!this.Handlers.TryGetValue(command.GetType(), out var taskGetter))
            {
                throw new NotSupportedException();
            }

            var task = taskGetter(command, cancellationToken, progress);
            if (!task.GetType().IsGenericType)
            {
                throw new InvalidOperationException();
            }

            await task.ConfigureAwait(false);
            var getter = typeof(Task<TResult>).GetProperty(nameof(Task<TResult>.Result));
            if (getter == null)
            {
                throw new InvalidOperationException();
            }

            var property = getter.GetGetMethod();
            if (property == null)
            {
                throw new InvalidOperationException();
            }

            return (TResult)property.Invoke(task, BindingFlags.Instance | BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
        }

        protected async Task Invoke<TCommand>(TCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null) where TCommand : UiCommand
        {
            if (!this.Handlers.TryGetValue(command.GetType(), out var taskGetter))
            {
                throw new NotSupportedException();
            }

            await taskGetter(command, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}