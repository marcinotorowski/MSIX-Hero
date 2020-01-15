using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public class BusyManager : IBusyManager
    {
        private readonly IList<IBusyContext> contexts = new List<IBusyContext>();
        private readonly object lockObject = new object();

        public IBusyContext Begin()
        {
            lock (lockObject)
            {
                var context = new ProgressBusyContext(this);
                this.contexts.Add(context);
                this.RefreshStatus();
                return context;
            }
        }

        public void End(IBusyContext context)
        {
            lock (lockObject)
            {
                contexts.Remove(context);
                this.RefreshStatus();
            }
        }

        public void ExecuteAsync(Action<IBusyContext> action)
        {
            var context = this.Begin();

            try
            {
                action(context);
            }
            finally
            {
                this.End(context);
            }
        }

        public async Task ExecuteAsync(Func<IBusyContext, Task> taskFactory)
        {
            var context = this.Begin();

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

        private void RefreshStatus()
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
                sc(this, new BusyStatusChange(true, context.Message, context.Progress));
            }
            else
            {
                sc(this, new BusyStatusChange(false, null, 100));
            }
        }

        public class ProgressBusyContext : IBusyContext
        {
            private readonly BusyManager manager;
            private string message;
            private int progress;

            public ProgressBusyContext(BusyManager manager)
            {
                this.manager = manager;
            }

            public string Message
            {
                get
                {
                    return this.message;
                }

                set
                {
                    this.message = value;
                    this.manager.RefreshStatus();
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
                    this.manager.RefreshStatus();
                }
            }

            public void Report(ProgressData value)
            {
                this.message = value.Message;
                this.progress = value.Progress;
                this.manager.RefreshStatus();
            }
        }
    }
}