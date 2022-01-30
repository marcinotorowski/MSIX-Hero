// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Mvvm.Progress
{
    public class BusyManager : IBusyManager
    {
        private static readonly LogSource Logger = new();        private readonly IList<IBusyContext> contexts = new List<IBusyContext>();
        private readonly object lockObject = new object();

        public IBusyContext Begin(OperationType type = OperationType.Other)
        {
            Logger.Verbose().WriteLine("Starting a context...");
            lock (lockObject)
            {
                var context = new ProgressBusyContext(this, type);
                this.contexts.Add(context);
                this.RefreshStatus(type);
                Logger.Verbose().WriteLine("Context started...");
                return context;
            }
        }

        public void End(IBusyContext context)
        {
            Logger.Verbose().WriteLine("Ending a context...");
            lock (lockObject)
            {
                contexts.Remove(context);
                this.RefreshStatus(context.Type);
                Logger.Verbose().WriteLine("Context ended...");
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(CommandManager.InvalidateRequerySuggested, DispatcherPriority.ApplicationIdle);
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
                Logger.Verbose().WriteLine($@"Notifying {context.Message}, {context.Progress}%...");
                sc(this, new BusyStatusChange(operationType, true, context.Message, context.Progress));
            }
            else
            {
                Logger.Verbose().WriteLine($@"Notifying 100%...");
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
                get => this.message;

                set
                {
                    this.message = value;
                    this.manager.RefreshStatus(this.Type);
                }
            }

            public int Progress
            {
                get => this.progress;

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