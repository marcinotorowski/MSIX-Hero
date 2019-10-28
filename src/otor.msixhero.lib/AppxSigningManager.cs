using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
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
            ps.AddScript(scriptPath);
            ps.AddParameter("PublisherFriendlyName", publisherDisplayName);
            ps.AddParameter("PublisherName", publisherName);
            ps.AddParameter("Password", password);
            ps.AddParameter("OutputDirectory", outputDirectory.FullName);
            // ps.AddParameter("PfxOutputFileName", null);
            // ps.AddParameter("CerOutputFileName", null);
            ps.AddParameter("CreatePasswordFile");

            EventHandler<DataAddedEventArgs> handler = null;

            if (progress != null)
            {
                handler = (sender, args) =>
                {
                    var progressRecords = (PSDataCollection<ProgressRecord>) sender;
                    var progressRecord = progressRecords.Last();
                    progress.Report(new Progress.ProgressData(progressRecord.PercentComplete, progressRecord.StatusDescription));
                };

                ps.Streams.Progress.DataAdded += handler;
            }

            ps.Streams.Error.DataAdded += (sender, args) =>
            {
                // todo: error;
            };

            try
            {
                await ps.InvokeAsync().ConfigureAwait(false);
                return true;
            }
            finally
            {
                if (handler != null)
                {
                    ps.Streams.Progress.DataAdded -= handler;
                }
            }
        }
    }
}