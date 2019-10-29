using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.PowerShellInterop
{
    public class PowerShellSession : IDisposable
    {
        private readonly PowerShell powerShell;
        private IProgress<Progress.ProgressData> progressReporter;
        private Exception exception;

        public void Dispose()
        {
            this.powerShell.Streams.Error.DataAdded -= this.ErrorOnDataAdded;
            this.powerShell.Streams.Progress.DataAdded -= this.ProgressOnDataAdded;
            this.powerShell.Dispose();
        }

        internal PowerShellSession()
        {
            this.powerShell = PowerShell.Create();
            this.powerShell.Streams.Error.DataAdded += this.ErrorOnDataAdded;
            this.powerShell.Streams.Progress.DataAdded += this.ProgressOnDataAdded;
        }

        private void ErrorOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (this.exception != null)
            {
                return;
            }

            var errorRecord = ((PSDataCollection<ErrorRecord>)sender).First();
            this.exception = errorRecord.Exception;
        }

        private void ProgressOnDataAdded(object sender, DataAddedEventArgs e)
        {
            if (this.progressReporter == null)
            {
                return;
            }

            var progressRecord = ((PSDataCollection<ProgressRecord>)sender).First();
            this.progressReporter.Report(new Progress.ProgressData(progressRecord.PercentComplete, progressRecord.StatusDescription ?? progressRecord.CurrentOperation));
        }

        public async Task InvokeAsync(IProgress<Progress.ProgressData> progress = null)
        {
            try
            {
                this.progressReporter = progress;
                await this.powerShell.InvokeAsync().ConfigureAwait(false);

                if (this.exception != null)
                {
                    throw exception;
                }
            }
            finally
            {
                this.progressReporter = null;
            }
        }

        public PowerShell AddCommand(string cmdlet, bool localScope = false)
        {
            return this.powerShell.AddCommand(cmdlet, localScope);
        }

        public static async Task<PowerShellSession> CreateForModule(string module = null, bool skipEditionCheck = false)
        {
            var session = new PowerShellSession();
            var cmd = session.AddCommand("Set-ExecutionPolicy");
            cmd.AddParameter("ExecutionPolicy", "ByPass");
            cmd.AddParameter("Scope", "Process");
            await cmd.InvokeAsync().ConfigureAwait(false);
            session.powerShell.Commands.Clear();

            if (!string.IsNullOrEmpty(module))
            {
                var cmd2 = session.AddCommand("Import-Module");
                cmd2.AddParameter("Name", module);
                if (skipEditionCheck)
                {
                    cmd2.AddParameter("SkipEditionCheck");
                }
            }

            return session;
        }

        public static Task<PowerShellSession> CreateForAppxModule()
        {
            return CreateForModule("Appx");
        }
    }
}
