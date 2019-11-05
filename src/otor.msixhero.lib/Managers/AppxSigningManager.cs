using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
{
    public class AppxSigningManager : IAppxSigningManager
    {
        public Task<bool> ExtractCertificateFromMsix(
            string msixFile, 
            string outputFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            return this.ExtractCertificateFromMsix(msixFile, false, outputFile, cancellationToken, progress);
        }

        public Task<bool> ImportCertificateFromMsix(
            string msixFile,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            return this.ExtractCertificateFromMsix(msixFile, true, null, cancellationToken, progress);
        }
        
        public async Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory, 
            string publisherName, 
            string publisherDisplayName, 
            string password,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            var scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "create-certificate.ps1");

            using var ps = await PowerShellInterop.PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
            using var cmd = ps.AddCommand(scriptPath, false);
            using var paramPublisherFriendlyName = cmd.AddParameter("PublisherFriendlyName", publisherDisplayName);
            using var paramPublisherName = cmd.AddParameter("PublisherName", publisherName);
            using var paramPassword = cmd.AddParameter("Password", password);
            using var paramOutputDirectory = cmd.AddParameter("OutputDirectory", outputDirectory.FullName);
            using var paramPfxOutputFileName = cmd.AddParameter("PfxOutputFileName", null);
            using var paramCerOutputFileName = cmd.AddParameter("CerOutputFileName", null);
            using var paramCreatePasswordFile = cmd.AddParameter("CreatePasswordFile");

            using var result = await ps.InvokeAsync(progress).ConfigureAwait(false);
            return true;
        }
        private async Task<bool> ExtractCertificateFromMsix(
            string msixFile,
            bool importToStore = false,
            string outputFile = null,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            var scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "extract-certificate-from-msix.ps1");

            using var ps = await PowerShellInterop.PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
            using var cmd = ps.AddCommand(scriptPath, false);
            using var paramSourceMsixFile = cmd.AddParameter("SourceMsixFile", msixFile);

            if (outputFile != null)
            {
                using var paramCerOutputFileName = cmd.AddParameter("CerOutputFileName", outputFile);
            }

            if (importToStore)
            {
                using var paramImportToStore = cmd.AddParameter("ImportToStore");
            }

            using var result = await ps.InvokeAsync(progress).ConfigureAwait(false);
            return true;
        }
    }
}