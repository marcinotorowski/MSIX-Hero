using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Commanding
{
    public interface ICommandBus
    {
        void SetStateManager(IWritableApplicationStateManager stateManager);

        void Execute(VoidCommand action);
        
        T GetExecute<T>(CommandWithOutput<T> action);

        Task ExecuteAsync(VoidCommand action, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null);

        Task<T> GetExecuteAsync<T>(CommandWithOutput<T> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null);

        Task<object> GetExecuteAsync(VoidCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null);
    }
}