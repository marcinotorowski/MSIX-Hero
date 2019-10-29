using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface ICommandExecutor
    {
        void Execute(BaseCommand action);

        Task ExecuteAsync(BaseCommand action, CancellationToken cancellationToken = default);

        T Execute<T>(BaseCommand action);

        Task<T> ExecuteAsync<T>(BaseCommand action, CancellationToken cancellationToken = default);
    }
}