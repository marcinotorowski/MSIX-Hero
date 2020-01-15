using System;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure
{
    public interface IBusyManager
    {
        void ExecuteAsync(Action<IBusyContext> action);

        Task ExecuteAsync(Func<IBusyContext, Task> taskFactory);

        IBusyContext Begin();

        void End(IBusyContext context);

        event EventHandler<IBusyStatusChange> StatusChanged;
    }
}
