using System;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Update
{
    public interface IUpdateChecker
    {
        Task<UpdateCheckResult> CheckForNewVersion(Version currentVersion);
        Task<UpdateCheckResult> CheckForNewVersion();
    }
}