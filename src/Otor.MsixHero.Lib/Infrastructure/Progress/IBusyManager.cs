using System;
using System.Threading.Tasks;

namespace Otor.MsixHero.Lib.Infrastructure.Progress
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
