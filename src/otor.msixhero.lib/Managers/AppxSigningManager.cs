using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
{
    public class AppxSigningManager : IAppxSigningManager
    {
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
    }
}