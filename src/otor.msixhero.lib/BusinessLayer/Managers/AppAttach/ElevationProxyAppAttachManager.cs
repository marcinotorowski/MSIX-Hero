using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Commands.Packages.AppAttach;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Managers.AppAttach
{
    public class ElevationProxyAppAttachManager : IAppAttachManager
    {
        private readonly IElevatedClient client;

        public ElevationProxyAppAttachManager(IElevatedClient client)
        {
            this.client = client;
        }

        public Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool extractCertificate, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null)
        {
            var cmd = new ConvertToVhd
            {
                VhdPath = volumePath,
                ExtractCertificate = extractCertificate, 
                GenerateScripts = generateScripts,
                PackagePath = packagePath,
                SizeInMegaBytes = vhdSize
            };

            return client.Execute(cmd, cancellationToken, progressReporter);
        }
    }
}
