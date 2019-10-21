using System;
using System.Text;
using System.Threading.Tasks;

namespace MSI_Hero.Services
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
