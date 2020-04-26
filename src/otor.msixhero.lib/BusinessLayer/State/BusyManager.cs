using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class BusyManager : IBusyManager
    {
        private static readonly ILog Logger = LogManager.GetLogger<BusyManager>();
        private readonly IList<IBusyContext> contexts = new List<IBusyContext>();
        private readonly object lockObject = new object();

        public IBusyContext Begin(OperationType type = OperationType.Other)
        {
            Logger.Trace("Starting a context...");
            lock (lockObject)
            {
                var context = new ProgressBusyContext(this, type);
                this.contexts.Add(context);
                this.RefreshStatus(type);
                Logger.Trace("Context started...");
                return context;
            }
        }

        public void End(IBusyContext context)
        {
            Logger.Trace("Ending a context...");
            lock (lockObject)
            {
                contexts.Remove(context);
                this.RefreshStatus(context.Type);
                Logger.Trace("Context ended...");
            }
        }

        public void ExecuteAsync(Action<IBusyContext> action, OperationType type = OperationType.Other)
        {
            var context = this.Begin(type);

            try
            {
                action(context);
            }
            finally
            {
                this.End(context);
            }
        }

        public async Task ExecuteAsync(Func<IBusyContext, Task> taskFactory, OperationType type = OperationType.Other)
        {
            var context = this.Begin(type);

            try
            {
                await taskFactory(context).ConfigureAwait(false);
            }
            finally
            {
                this.End(context);
            }
        }

        public event EventHandler<IBusyStatusChange> StatusChanged;

        private void RefreshStatus(OperationType operationType)
        {
            var sc = this.StatusChanged;
            if (sc == null)
            {
                return;
            }

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            var context = this.contexts.LastOrDefault();
            if (context != null)
            {
                Logger.Trace($@"Notifying ({{true}}, {context.Message}, {context.Progress}%");
                sc(this, new BusyStatusChange(operationType, true, context.Message, context.Progress));
            }
            else
            {
                Logger.Trace($@"Notifying ({{false}}, 100%");
                sc(this, new BusyStatusChange(operationType, false, null, 100));
            }
        }

        public class ProgressBusyContext : IBusyContext
        {
            private readonly BusyManager manager;
            private string message;
            private int progress = -1;

            public ProgressBusyContext(BusyManager manager, OperationType operationType)
            {
                this.manager = manager;
                this.Type = operationType;
            }

            public OperationType Type { get; }

            public string Message
            {
                get
                {
                    return this.message;
                }

                set
                {
                    this.message = value;
                    this.manager.RefreshStatus(this.Type);
                }
            }

            public int Progress
            {
                get
                {
                    return this.progress;
                }

                set
                {
                    this.progress = value;
                    this.manager.RefreshStatus(this.Type);
                }
            }

            public void Report(ProgressData value)
            {
                this.message = value.Message;
                this.progress = value.Progress;
                this.manager.RefreshStatus(this.Type);
            }
        }
    }
}