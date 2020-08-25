using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto;

namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop
{
    public class AppAttachManagerElevationProxy : IAppAttachManager
    {
        private readonly IElevatedClient client;

        public AppAttachManagerElevationProxy(IElevatedClient client)
        {
            this.client = client;
        }

        public Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool extractCertificate, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            var cmd = new CreateVolumeDto
            {
                VhdPath = volumePath,
                ExtractCertificate = extractCertificate, 
                GenerateScripts = generateScripts,
                PackagePath = packagePath,
                SizeInMegaBytes = vhdSize
            };

            return client.Invoke(cmd, cancellationToken, progressReporter);
        }
    }
}
