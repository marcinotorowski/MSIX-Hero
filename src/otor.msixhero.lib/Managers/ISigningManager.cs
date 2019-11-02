using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
{
    public interface IAppxSigningManager
    {
        Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory,
            string publisherName,
            string publisherDisplayName,
            string password,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null);
    }
}
