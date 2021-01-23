// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Diagnostic.Registry
{
    public class RegistryManager : IRegistryManager
    {
        public Task<RegistryMountState> GetRegistryMountState(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.GetRegistryMountState(package.InstallLocation, package.Name, cancellationToken);
        }

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
                using (var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("MSIX-Hero-" + packageName))
                {
                    if (reg != null)
                    {
                        hasRegistry = RegistryMountState.Mounted;
                    }
                    else
                    {
                        hasRegistry = RegistryMountState.NotMounted;
                    }
                }
            }

            return Task.FromResult(hasRegistry);
        }

        public Task DismountRegistry(InstalledPackage package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.DismountRegistry(package.Name, cancellationToken, progress);
        }

        public Task DismountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
                var proc = new ProcessStartInfo("cmd.exe", @"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
                Console.WriteLine(@"/c REG UNLOAD HKLM\MSIX-Hero-" + packageName);
                proc.UseShellExecute = true;
                proc.Verb = "runas";

                var p = Process.Start(proc);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start process for un-mounting");
                }

                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    throw new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code.");
                }
            },
            cancellationToken);
        }

        public Task MountRegistry(InstalledPackage package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return this.MountRegistry(package.Name, package.InstallLocation, startRegedit, cancellationToken);
        }

        public Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            return Task.Run(() =>
            {
                if (startRegedit)
                {
                    try
                    {
                        SetRegistryValue(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", "LastKey", @"Computer\HKEY_LOCAL_MACHINE\MSIX-Hero-" + packageName + @"\REGISTRY");
                    }
                    catch (Exception)
                    {
                        // whatever...
                    }
                }

                Console.WriteLine(@"/c REG LOAD HKLM\MSIX-Hero-" + packageName);
                var proc = new ProcessStartInfo("cmd.exe", @"/c REG LOAD HKLM\MSIX-Hero-" + packageName + " \"" + Path.Combine(installLocation, "Registry.dat") + "\"")
                {
                    UseShellExecute = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                var p = Process.Start(proc);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start process for mounting");
                }

                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    throw new InvalidOperationException($"cmd.exe returned {p.ExitCode} exit code.");
                }

                if (!startRegedit)
                {
                    return;
                }

                proc = new ProcessStartInfo("regedit.exe")
                {
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(proc);
            },
            cancellationToken);
        }

        private static void SetRegistryValue(string key, string name, string value)
        {
            RegistryKey regKey = null;

            try
            {
                regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key);
                if (regKey == null)
                {
                    regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key);
                }

                if (regKey == null)
                {
                    throw new InvalidOperationException();
                }

                if (!String.IsNullOrEmpty(value))
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