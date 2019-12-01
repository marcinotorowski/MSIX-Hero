using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Signing
{
    public interface IAppxSigningManager
    {
        Task<bool> ExtractCertificateFromMsix(string msixFile, string outputFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<bool> ExtractCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<PersonalCertificate> GetCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task SignPackage(string package, bool updatePublisher, string pfxPath, SecureString password, string timestampUrl = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);
        
        Task SignPackage(string package, bool updatePublisher, PersonalCertificate certificate, string timestampUrl = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<bool> ImportCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<bool> InstallCertificate(string certificateFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<bool> CreateSelfSignedCertificate(DirectoryInfo outputDirectory, string publisherName, string publisherDisplayName, string password, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<List<PersonalCertificate>> GetCertificatesFromStore(CertificateStoreType certificateStoreType, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);
    }
}
