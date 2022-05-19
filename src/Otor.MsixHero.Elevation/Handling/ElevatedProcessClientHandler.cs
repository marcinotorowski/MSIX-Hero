// MIT License
// 
// Copyright(c) 2022 Marcin Otorowski
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// https://github.com/marcinotorowski/simpleelevation/blob/develop/LICENSE.md

using System.Diagnostics;
using Dapplo.Log;

namespace Otor.MsixHero.Elevation.Handling;

public class ElevatedProcessClientHandler : ClientHandler
{
    private static readonly LogSource Log = new LogSource();

    private readonly HashSet<Process> _processes = new HashSet<Process>();

    private readonly string _executablePath;
    private readonly string _argumentsSingleRun;
    private readonly string _argumentsKeepActive;

    public ElevatedProcessClientHandler(string executablePath, string argumentsSingleRun, string argumentsKeepActive)
    {
        this._executablePath = executablePath;
        this._argumentsSingleRun = argumentsSingleRun;
        this._argumentsKeepActive = argumentsKeepActive;
    }

    public override ValueTask<bool> IsUacHelperRunning()
    {
        Log.Debug().WriteLine("UAC Client -> Checking if UAC helper process is running...");
        var currentPid = Process.GetCurrentProcess().Id;
        var target = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(this._executablePath)).FirstOrDefault(p => p.Id != currentPid);

        if (target != null)
        {
            Log.Debug().WriteLine("UAC Client -> UAC helper is running with PID = {0}...", target.Id);
        }
        else
        {
            Log.Debug().WriteLine("UAC Client -> No matching process was found.");
        }

        return ValueTask.FromResult(target != null);
    }

    public override async ValueTask<bool> StartServerAsync(bool keepElevation)
    {
        Log.Info().WriteLine("UAC Client -> Starting server...");
        Log.Debug().WriteLine("UAC Client -> Starting elevation helper '{0}' {1}", this._executablePath, keepElevation ? this._argumentsKeepActive : this._argumentsSingleRun);
        var psi = new ProcessStartInfo(this._executablePath, keepElevation ? this._argumentsKeepActive : this._argumentsSingleRun)
        {
            Verb = "runas",
            UseShellExecute = true,
#if !DEBUG
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
#else
            WindowStyle = ProcessWindowStyle.Normal,
            CreateNoWindow = false
#endif
        };

        this._processes.Add(this.Start(psi));

        return await this.IsUacHelperRunning().ConfigureAwait(false);
    }

    private Process Start(ProcessStartInfo info)
    {
        var newProcess = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true
        };

        if (newProcess == null)
        {
            throw new InvalidOperationException("Could not start the process.");
        }
        
        _processes.Add(newProcess);
        newProcess.Exited += (_, _) =>
        {
            Log.Info().WriteLine("UAC Client -> Server process has finished.");
            _processes.Remove(newProcess);
        };

        if (!newProcess.Start())
        {
            throw new InvalidOperationException($"Could not start '{info.FileName}'.");
        }

        return newProcess;
    }

    ~ElevatedProcessClientHandler()
    {
        Dispose(false);
    }

    public override void Dispose()
    {
        base.Dispose();
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await this.DisposeAsync(true).ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    // ReSharper disable once UnusedParameter.Local
    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        foreach (var process in this._processes.ToArray())
        {
            try
            {
                if (!process.HasExited)
                {
                    Log.Debug().WriteLine("UAC Client -> Killing dangling process PID = {0}", process.Id);
                    process.Kill();
                }

                this._processes.Remove(process);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private ValueTask DisposeAsync(bool disposing)
    {
        if (!disposing)
        {
            return ValueTask.CompletedTask;
        }

        foreach (var process in this._processes.ToArray())
        {
            try
            {
                if (!process.HasExited)
                {
                    Log.Debug().WriteLine("UAC Client -> Killing dangling process PID = {0}", process.Id);
                    process.Kill();
                }

                this._processes.Remove(process);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        return ValueTask.CompletedTask;
    }
}