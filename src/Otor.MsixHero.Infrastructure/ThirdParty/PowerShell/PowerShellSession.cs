// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.ThirdParty.PowerShell
{
    public class PowerShellSession : IDisposable
    {
        private readonly System.Management.Automation.PowerShell powerShell;
        private IProgress<ProgressData> progressReporter;
        private Exception exception;

        public void Dispose()
        {
            this.powerShell.Streams.Error.DataAdded -= this.ErrorOnDataAdded;
            this.powerShell.Streams.Progress.DataAdded -= this.ProgressOnDataAdded;
            this.powerShell.Dispose();
        }

        internal PowerShellSession()
        {
            this.powerShell = System.Management.Automation.PowerShell.Create();
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
            this.progressReporter.Report(new ProgressData(progressRecord.PercentComplete, progressRecord.StatusDescription ?? progressRecord.CurrentOperation));
        }

        public async Task<PSDataCollection<PSObject>> InvokeAsync(IProgress<ProgressData> progress = null)
        {
            try
            {
                this.progressReporter = progress;
                var result = await this.powerShell.InvokeAsync().ConfigureAwait(false);

                if (this.exception != null)
                {
                    throw exception;
                }

                return result;
            }
            finally
            {
                this.progressReporter = null;
            }
        }

        public System.Management.Automation.PowerShell AddCommand(string cmdlet, bool localScope = false)
        {
            return this.powerShell.AddCommand(cmdlet, localScope);
        }

        public System.Management.Automation.PowerShell AddScript(string cmdlet, bool localScope = false)
        {
            return this.powerShell.AddScript(cmdlet, localScope);
        }

        public static async Task<PowerShellSession> CreateForModule(string module = null, bool skipEditionCheck = false)
        {
            var session = new PowerShellSession();
            var cmd = session.AddCommand("Set-ExecutionPolicy");
            cmd.AddParameter("ExecutionPolicy", "Bypass");
            cmd.AddParameter("Scope", "Process");
            await session.InvokeAsync().ConfigureAwait(false);
            session.powerShell.Commands.Clear();

            if (!string.IsNullOrEmpty(module))
            {
                var cmd2 = session.AddCommand("Import-Module");
                cmd2.AddParameter("Name", module);
                
                if (string.Equals(module, "Appx", StringComparison.OrdinalIgnoreCase))
                {
                    cmd2.AddParameter("UseWindowsPowershell");
                }
                
                if (skipEditionCheck)
                {
                    cmd2.AddParameter("SkipEditionCheck");
                }

                await session.InvokeAsync().ConfigureAwait(false);
                session.powerShell.Commands.Clear();
            }

            return session;
        }

        public static Task<PowerShellSession> CreateForAppxModule()
        {
            return CreateForModule("Appx");
        }
    }
}
