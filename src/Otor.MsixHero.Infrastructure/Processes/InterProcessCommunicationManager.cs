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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Processes
{
    public sealed class InterProcessCommunicationManager : IInterProcessCommunicationManager
    {
        private static readonly AutoResetEvent SafeHandle = new AutoResetEvent(true);

        private readonly HashSet<Process> processes = new HashSet<Process>();

        public async Task<bool> Test(CancellationToken cancellationToken = default)
        {
            var process = await GetProcessIfRunning(cancellationToken).ConfigureAwait(false);
            return process != null;
        }

        public async Task<NamedPipeClientStream> GetCommunicationChannel(CancellationToken cancellationToken = default)
        {
            var process = Process.GetProcessesByName("msixhero-uac").FirstOrDefault();

            if (process == null || process.HasExited)
            {
                // make double checking with help of mutex
                if (!SafeHandle.WaitOne(TimeSpan.FromSeconds(30)))
                {
                    throw new InvalidOperationException("Could not get exclusive access.");
                }

                try
                {
                    process = Process.GetProcessesByName("msixhero-uac").FirstOrDefault();
                    if (process == null || process.HasExited)
                    {
                        var psi = new ProcessStartInfo(System.IO.Path.Join(AppDomain.CurrentDomain.BaseDirectory, "msixhero-uac.exe"), "--selfElevate")
                        {
                            Verb = "runas",
                            UseShellExecute = true,
#if RELEASE
                            WindowStyle = ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
#endif
                        };

                        var p = this.Start(psi);
                        if (p == null)
                        {
                            throw new InvalidOperationException("Could not start the helper.");
                        }

                        await Task.Delay(200, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SafeHandle.Set();
                }
            }

            return new NamedPipeClientStream("msixhero");
        }

        private static Task<Process> GetProcessIfRunning(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Process.GetProcessesByName("msixhero-uac").FirstOrDefault());
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

        ~InterProcessCommunicationManager()
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
