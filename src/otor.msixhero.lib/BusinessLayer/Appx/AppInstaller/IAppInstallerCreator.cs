using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.AppInstaller
{
    public interface IAppInstallerCreator
    {
        Task Create(AppInstallerConfig config, string file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}
