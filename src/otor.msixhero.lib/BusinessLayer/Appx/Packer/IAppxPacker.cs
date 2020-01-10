using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Packer
{
    public interface IAppxPacker
    {
        Task Pack(string directory, string packagePath, bool compress = true, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
        Task Unpack(string packagePath, string directory, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
    }
}
