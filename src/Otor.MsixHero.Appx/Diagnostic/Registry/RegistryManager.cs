// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Otor.MsixHero.Appx.Diagnostic.Registry.Enums;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Diagnostic.Registry
{
    public class RegistryManager : IRegistryManager
    {
        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            RegistryMountState hasRegistry;

            if (string.IsNullOrEmpty(installLocation))
            {
                hasRegistry = RegistryMountState.NotApplicable;
            }
            else if (!File.Exists(Path.Combine(installLocation, "registry.dat")))
            {
                hasRegistry = RegistryMountState.NotApplicable;
            }
            else
            {
                using var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("MSIX-Hero-" + packageName);
                hasRegistry = reg != null ? RegistryMountState.Mounted : RegistryMountState.NotMounted;
            }

            return Task.FromResult(hasRegistry);
        }
        
        public Task DismountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proc = new ProcessStartInfo("cmd.exe", "/c REG UNLOAD " + CommandLineHelper.EncodeParameterArgument(@"HKLM\MSIX-Hero-" + packageName));
            Console.WriteLine(@"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
            proc.UseShellExecute = true;
            proc.Verb = "runas";

            var p = new Process
            {
                StartInfo = proc,
                EnableRaisingEvents = true
            };
            
            var tcs = new TaskCompletionSource();

            p.Exited += (_, _) =>
            {
                if (p.ExitCode == 0)
                {
                    tcs.TrySetException(new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code."));
                }
                else
                {
                    tcs.TrySetResult();
                }
            };

            if (!p.Start())
            {
                tcs.TrySetException(new InvalidOperationException("Could not start cmd.exe."));
            }

            return tcs.Task;
        }
        
        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (startRegedit)
            {
                ExceptionGuard.Guard(() => SetRegistryValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "LastKey", @"Computer\HKEY_LOCAL_MACHINE\MSIX-Hero-" + packageName + @"\REGISTRY"));
            }

            var proc = new ProcessStartInfo("cmd.exe",
                @"/c REG LOAD " + CommandLineHelper.EncodeParameterArgument(@"HKLM\MSIX-Hero-" + packageName) + " " +
                CommandLineHelper.EncodeParameterArgument(Path.Combine(installLocation, "Registry.dat")))
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                Verb = "runas"
            };

            var p = new Process
            {
                StartInfo = proc,
                EnableRaisingEvents = true
            };
            
            var tcs = new TaskCompletionSource();
            
            if (startRegedit)
            {
                tcs.Task.ContinueWith(_ =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    proc = new ProcessStartInfo("regedit.exe")
                    {
                        Verb = "runas",
                        UseShellExecute = true
                    };

                    Process.Start(proc);
                }, cancellationToken);
            }

            p.Exited += (_, _) =>
            {
                if (p.ExitCode == 0)
                {
                    tcs.TrySetResult();
                }
                else
                {
                    tcs.TrySetException(new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code."));
                }
            };

            if (!p.Start())
            {
                tcs.TrySetException(new InvalidOperationException("Could not start cmd.exe."));
            }

            return tcs.Task;
        }

        private static void SetRegistryValue(string key, string name, string value)
        {
            RegistryKey regKey = null;

            try
            {
                regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key) ?? Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key);

                if (regKey == null)
                {
                    throw new InvalidOperationException();
                }

                if (!string.IsNullOrEmpty(value))
                {
                    regKey.SetValue(name, value, RegistryValueKind.String);
                }
                else
                {
                    regKey.DeleteValue(name, false);
                }
            }
            finally
            {
                if (regKey != null)
                {
                    regKey.Dispose();
                }
            }
        }
    }
}