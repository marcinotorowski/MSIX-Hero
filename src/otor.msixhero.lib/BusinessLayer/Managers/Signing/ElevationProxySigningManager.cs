using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.SelfElevation;

namespace otor.msixhero.lib.BusinessLayer.Managers.Signing
{
    public class ElevationProxySigningManager : ISigningManager
    {
        private readonly ISelfElevationManagerFactory<ISigningManager> managerFactory;
        private readonly IElevatedClient client;

        public ElevationProxySigningManager(IElevatedClient client, ISelfElevationManagerFactory<ISigningManager> managerFactory)
        {
            this.managerFactory = managerFactory;
            this.client = client;
        }

        public Task InstallCertificate(string certificateFilePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return client.Execute(new InstallCertificate(certificateFilePath), cancellationToken, progress);
        }

        public async Task ImportCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ImportCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task CreateSelfSignedCertificate(DirectoryInfo outputDirectory, string publisherName, string publisherDisplayName, string password, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.CreateSelfSignedCertificate(outputDirectory, publisherName, publisherDisplayName, password, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<List<PersonalCertificate>> GetCertificatesFromStore(CertificateStoreType certificateStoreType, bool onlyValid = true, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetCertificatesFromStore(certificateStoreType, onlyValid, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task ExtractCertificateFromMsix(string msixFile, string outputFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ExtractCertificateFromMsix(msixFile, outputFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task ExtractCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ExtractCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<PersonalCertificate> GetCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task SignPackage(string package, bool updatePublisher, string pfxPath, SecureString password, string timestampUrl = null, IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SignPackage(package, updatePublisher, pfxPath, password, timestampUrl, increaseVersion, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task SignPackage(string package, bool updatePublisher, PersonalCertificate certificate, string timestampUrl = null, IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.Get(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SignPackage(package, updatePublisher, certificate, timestampUrl, increaseVersion, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}