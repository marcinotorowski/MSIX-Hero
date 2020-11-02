using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;

namespace Otor.MsixHero.Appx.Packaging.Manifest
{
    public interface IAppxManifestReader
    {
        Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default);

        Task<AppxPackage> Read(IAppxFileReader fileReader, bool resolveDependencies, CancellationToken cancellationToken = default);

        Task<AppxBundle> ReadBundle(IAppxFileReader fileReader, CancellationToken cancellationToken = default);
    }
}