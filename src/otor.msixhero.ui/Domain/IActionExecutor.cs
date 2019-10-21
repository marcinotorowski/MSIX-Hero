using System.Threading;
using System.Threading.Tasks;
using MSI_Hero.Domain.Actions;
using MSI_Hero.Domain.State;

namespace MSI_Hero.Domain
{
    public interface IActionExecutor
    {
        bool Execute(IAction action);

        Task<bool> ExecuteAsync(IAction action, CancellationToken cancellationToken = default(CancellationToken));
    }
}