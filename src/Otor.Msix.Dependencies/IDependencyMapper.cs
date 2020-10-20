using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.Msix.Dependencies.Domain;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.Msix.Dependencies
{
    public interface IDependencyMapper
    {
        Task<DependencyGraph> GetGraph(string initialPackage, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<DependencyGraph> GetGraph(AppxPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);
    }
}