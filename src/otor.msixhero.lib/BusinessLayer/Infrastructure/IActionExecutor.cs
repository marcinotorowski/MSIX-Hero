using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;

namespace otor.msixhero.lib.BusinessLayer.Infrastructure
{
    public interface IActionExecutor
    {
        bool Execute(BaseAction action);

        Task<bool> ExecuteAsync(BaseAction action, CancellationToken cancellationToken = default(CancellationToken));
    }
}