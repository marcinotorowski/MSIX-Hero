using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib
{
    public class AppxSigningManager : IAppxSigningManager
    {
        public async Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory, 
            string publisherName, 
            string publisherDisplayName, 
            string password,
            CancellationToken cancellationToken = default,
            IProgress<Progress.ProgressData> progress = null)
        {
            using var ps = await PowerShellInterop.PowerShellSession.CreateForModule("PKI", true).ConfigureAwait(false);
            var scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "create-certificate.ps1");
            var cmd = ps.AddCommand(scriptPath, false);
            cmd.AddParameter("PublisherFriendlyName", publisherDisplayName);
            cmd.AddParameter("PublisherName", publisherName);
            cmd.AddParameter("Password", password);
            cmd.AddParameter("OutputDirectory", outputDirectory.FullName);
            cmd.AddParameter("PfxOutputFileName", null);
            cmd.AddParameter("CerOutputFileName", null);
            cmd.AddParameter("CreatePasswordFile");

            await ps.InvokeAsync(progress).ConfigureAwait(false);
            return true;
        }
    }
}