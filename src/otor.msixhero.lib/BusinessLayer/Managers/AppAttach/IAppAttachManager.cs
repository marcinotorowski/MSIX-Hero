using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Managers.AppAttach
{
    public interface IAppAttachManager : ISelfElevationAwareManager
    {
        Task CreateVolume(
            string packagePath, 
            string volumePath, 
            uint vhdSize, 
            bool extractCertificate,
            bool generateScripts, 
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progressReporter = null);
    }
}
