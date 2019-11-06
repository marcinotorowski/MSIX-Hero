using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
{
    public interface IAppxSigningManager
    {
        Task<bool> ExtractCertificateFromMsix(
            string msixFile, 
            string outputFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null);

        Task<bool> ImportCertificateFromMsix(
            string msixFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null);

        Task<bool> InstallCertificate(
            string certificateFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null);

        Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory,
            string publisherName,
            string publisherDisplayName,
            string password,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null);
    }
}
