using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Packer.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.Packer
{
    public interface IAppxPacker
    {
        Task Pack(string directory, string packagePath, AppxPackerOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
        
        Task PackFiles(string directory, string packagePath, AppxPackerOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
       
        Task Unpack(string packagePath, string directory, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
    }
}
