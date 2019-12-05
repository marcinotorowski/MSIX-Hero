using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Signing
{
    public interface IAppxPacker
    {
        Task Pack(string directory, string packagePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
        Task Unpack(string packagePath, string directory, CancellationToken cancellationToken = default, IProgress<ProgressData> progess = default);
    }
}
