using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public sealed class ProcessManager : IProcessManager
    {
        private static readonly AutoResetEvent SafeHandle = new AutoResetEvent(true);

        private readonly HashSet<Process> processes = new HashSet<Process>();

        public async Task Connect(CancellationToken cancellationToken = default)
        {
            var process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();

            if (process == null || process.HasExited)
            {
                // make double checking with help of mutex
                if (!SafeHandle.WaitOne(TimeSpan.FromSeconds(30)))
                {
                    throw new InvalidOperationException("Could not get exclusive access.");
                }

                try
                {
                    process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();
                    if (process == null || process.HasExited)
                    {
                        var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper.exe"), "--selfElevate")
                        {
                            Verb = "runas",
                            UseShellExecute = true,
                            // WindowStyle = ProcessWindowStyle.Hidden,
                            // CreateNoWindow = true
                        };

                        var p = this.Start(psi);
                        if (p == null)
                        {
                            throw new InvalidOperationException("Could not start the helper.");
                        }

                        await Task.Delay(400, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SafeHandle.Set();
                }
            }
        }

        private Process Start(ProcessStartInfo info)
        {
            var newProcess = Process.Start(info);
            if (newProcess == null)
            {
                throw new InvalidOperationException("Could not start the process.");
            }

            newProcess.EnableRaisingEvents = true;
            processes.Add(newProcess);
            newProcess.Exited += (sender, e) =>
            {
                processes.Remove(newProcess);
            };

            return newProcess;
        }

        ~ProcessManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            foreach (var process in this.processes.ToArray())
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                    this.processes.Remove(process);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
            }
        }
    }
}
