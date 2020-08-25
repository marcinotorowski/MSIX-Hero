using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Updates
{
    public interface IUpdateConfigurationManager
    {
        Task<UpdateCheckResult> GetReleaseNotes(CancellationToken cancellationToken = default);
        
        Task<bool> ShouldShowReleaseNotes(bool markReleaseNotesAsShown = true, CancellationToken cancellation = default);
        
        Task DisableReleaseNotes(CancellationToken token = default);
    }
}