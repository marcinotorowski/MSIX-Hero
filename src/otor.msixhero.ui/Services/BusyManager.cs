using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Events;

namespace MSI_Hero.Services
{
    public class BusyManager : IBusyManager
    {
        private readonly IEventAggregator eventAggregator;

        private readonly IList<IBusyContext> contexts = new List<IBusyContext>();

        public BusyManager(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public IBusyContext Begin()
        {
            var context = new BusyContext(this);
            this.contexts.Add(context);
            this.RefreshStatus();
            return context;
        }

        public void End(IBusyContext context)
        {
            contexts.Remove(context);
            this.RefreshStatus();
        }

        public void Execute(Action<IBusyContext> action)
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

        public async Task Execute(Func<IBusyContext, Task> taskFactory)
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

        public class BusyContext : IBusyContext
        {
            private readonly BusyManager manager;
            private string message;
            private int progress;

            public BusyContext(BusyManager manager)
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
        }


        public class BusyStatusChange : IBusyStatusChange
        {
            public BusyStatusChange(bool isBusy, string message, int progress)
            {
                IsBusy = isBusy;
                Message = message;
                Progress = progress;
            }

            public bool IsBusy { get; private set; }

            public string Message { get; private set; }

            public int Progress { get; private set; }
        }
    }
}