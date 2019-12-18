using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.ModificationPackage;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Builder
{
    public interface IAppxContentBuilder
    {
        Task Create(AppInstallerConfig config, string file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Tuple<int, string> GetMinimumSupportedWindowsVersion(AppInstallerConfig config);

        Task Create(ModificationPackageConfig config, string file, ModificationPackageBuilderAction action, CancellationToken cancellation = default, IProgress<ProgressData> progress = default);
    }
}
