using System;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Updates
{
    public interface IUpdateChecker
    {
        Task<UpdateCheckResult> CheckForNewVersion(Version currentVersion);

        Task<UpdateCheckResult> CheckForNewVersion();
    }
}