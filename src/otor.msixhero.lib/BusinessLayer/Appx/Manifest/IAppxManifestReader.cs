using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest
{
    public interface IAppxManifestReader
    {
        Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default);
    }
}