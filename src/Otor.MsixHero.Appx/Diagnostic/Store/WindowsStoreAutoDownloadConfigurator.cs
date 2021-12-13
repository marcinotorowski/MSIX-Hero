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
using Microsoft.Win32;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;

namespace Otor.MsixHero.Appx.Diagnostic.Store;

public class WindowsStoreAutoDownloadConfigurator : IWindowsStoreAutoDownloadConfigurator
{
    public WindowsStoreAutoDownload Get()
    {
        using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var subKey = key.OpenSubKey(@"SOFTWARE\Policies\Microsoft\WindowsStore");

        if (subKey == null)
        {
            return WindowsStoreAutoDownload.Default;
        }

        var allowAllTrustedApps = subKey.GetValue("AutoDownload");

        if (allowAllTrustedApps == null)
        {
            return WindowsStoreAutoDownload.Default;
        }

        var dword = allowAllTrustedApps is int dwordValue1 ? dwordValue1 : 0;

        switch (dword)
        {
            case 2:
                return WindowsStoreAutoDownload.Never;
            case 4:
                return WindowsStoreAutoDownload.Always;
            default:
                return WindowsStoreAutoDownload.Default;
        }
    }

    public bool Set(WindowsStoreAutoDownload status)
    {
        var newValue = (int)status;
        string cmd;

        switch (newValue)
        {
            case 4:
            case 2:
                if ((int)this.Get() == newValue)
                {
                    return true;
                }

                cmd = @"ADD HKLM\SOFTWARE\Policies\Microsoft\WindowsStore /v AutoDownload /t REG_DWORD /f /d " + newValue;
                break;

            default:
                if (this.Get() == WindowsStoreAutoDownload.Default)
                {
                    return true;
                }

                cmd = @"DELETE HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsStore /f /v AutoDownload";
                break;
        }

        var psi = new ProcessStartInfo("reg.exe")
        {
            Arguments = cmd,
            UseShellExecute = true,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System),
            Verb = "runas",
#if !DEBUG
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
#endif
        };

        try
        {
            var p = Process.Start(psi);
            if (p == null)
            {
                return false;
            }

            if (!p.WaitForExit(2000))
            {
                return false;
            }

            if (p.ExitCode != 0)
            {
                return false;
            }

        }
        catch (Exception)
        {
            return false;
        }

        return this.Get() == status;
    }

}