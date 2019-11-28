using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Commands;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public interface ICommandExecutor
    {
        void Execute(BaseCommand action);
        
        T GetExecute<T>(BaseCommand<T> action);

        Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default);

        Task<T> GetExecuteAsync<T>(BaseCommand<T> action, CancellationToken cancellationToken = default);
    }
}