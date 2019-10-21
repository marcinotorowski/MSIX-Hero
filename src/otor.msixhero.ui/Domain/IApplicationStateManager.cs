using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Domain.State;
using Prism.Events;

namespace MSI_Hero.Domain
{
    public interface IApplicationStateManager
    {
        IApplicationState CurrentState { get; }

        IActionExecutor Executor { get; }

        IEventAggregator EventAggregator { get; }
    }

    public interface IApplicationStateManager<out T> : IApplicationStateManager where T : IApplicationState
    {
        new T CurrentState { get; }
    }
}
