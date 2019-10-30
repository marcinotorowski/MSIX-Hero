using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface ICommandExecutor
    {
        void Execute(BaseCommand action);
        
        T GetExecute<T>(BaseCommand<T> action);

        Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default);

        Task<T> GetExecuteAsync<T>(BaseCommand<T> action, CancellationToken cancellationToken = default);
    }
}