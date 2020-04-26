using System;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure
{
    public interface IBusyManager
    {
        void ExecuteAsync(Action<IBusyContext> action, OperationType type = OperationType.Other);

        Task ExecuteAsync(Func<IBusyContext, Task> taskFactory, OperationType type = OperationType.Other);

        IBusyContext Begin(OperationType type = OperationType.Other);

        void End(IBusyContext context);

        event EventHandler<IBusyStatusChange> StatusChanged;
    }
}
