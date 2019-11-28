using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public sealed class ProcessManager : IProcessManager
    {
        private readonly HashSet<Process> processes = new HashSet<Process>();

        public Process Start(ProcessStartInfo info)
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
