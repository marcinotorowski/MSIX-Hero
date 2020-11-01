using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Dependencies.Domain;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Dependencies
{
    public interface IDependencyMapper
    {
        Task<DependencyGraph> GetGraph(string initialPackage, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<DependencyGraph> GetGraph(AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);
    }
}