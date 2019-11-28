using System;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure
{
    public interface IBusyManager
    {
        void Execute(Action<IBusyContext> action);

        Task Execute(Func<IBusyContext, Task> taskFactory);

        IBusyContext Begin();

        void End(IBusyContext context);

        event EventHandler<IBusyStatusChange> StatusChanged;
    }
}
