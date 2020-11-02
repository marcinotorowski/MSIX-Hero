using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.ModificationPackages
{
    public interface IAppxContentBuilder
    {
        Task Create(ModificationPackageConfig config, string file, ModificationPackageBuilderAction action, CancellationToken cancellation = default, IProgress<ProgressData> progress = default);
    }
}
