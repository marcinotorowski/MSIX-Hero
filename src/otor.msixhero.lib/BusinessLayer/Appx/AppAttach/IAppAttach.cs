using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.AppAttach
{
    public interface IAppAttach
    {
        Task CreateVolume(string packagePath, string volumePath, uint vhdSize, bool generateScripts, CancellationToken cancellationToken = default, IProgress<ProgressData> progressReporter = null);
    }
}
