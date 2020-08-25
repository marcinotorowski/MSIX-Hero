using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach
{
    public interface IAppAttachManager : ISelfElevationAware
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
