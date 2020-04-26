using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode
{
    public class RegistrySideloadingChecker : ISideloadingChecker
    {
        public bool SetStatus(SideloadingStatus status)
        {
            var allowSideloadDword = status != SideloadingStatus.NotAllowed ? 1 : 0;
            var allowDevModeDword = status == SideloadingStatus.DeveloperMode ? 1 : 0;

            var cmd1 = $@"reg.exe add HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock /t REG_DWORD /f /v AllowAllTrustedApps /d {allowSideloadDword}";
            var cmd2 = $@"reg.exe add HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock /t REG_DWORD /f /v AllowDevelopmentWithoutDevLicense /d {allowDevModeDword}";

            var psi = new ProcessStartInfo("cmd.exe")
            {
                Arguments = "/c \"" + cmd1 + " && " + cmd2 + "\"",
                UseShellExecute = true,
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System),
                Verb = "runas",
#if !DEBUG
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
#endif
            };

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

            return this.GetStatus() == status;

            // using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
               
            // RegistryKey subKey = null;
            // try
            // {
            //     subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
            //     if (subKey == null)
            //     {
            //         subKey = key.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
            //         if (subKey == null)
            //         {
            //             throw new InvalidOperationException("Could not create Registry Key to enable sideloading.");
            //         }
            //     }
               
            //     var regAllowAllTrustedApps = subKey.GetValue("AllowAllTrustedApps");
            //     var regAllowDevelopmentWithoutDevLicense = subKey.GetValue("AllowDevelopmentWithoutDevLicense");
               
            //     var requiredAllowAllTrustedApps = status >= SideloadingStatus.Sideloading;
            //     var requiredAllowDevelopmentWithoutDevLicense = status >= SideloadingStatus.DeveloperMode;
            //     var actualAllowAllTrustedApps = regAllowAllTrustedApps != null && regAllowAllTrustedApps is int value1 && value1 > 0;
            //     var actualAllowDevelopmentWithoutDevLicense = regAllowDevelopmentWithoutDevLicense != null && regAllowDevelopmentWithoutDevLicense is int value2 && value2 > 0;
               
            //     if (actualAllowAllTrustedApps != requiredAllowAllTrustedApps)
            //     {
            //         subKey.SetValue("AllowAllTrustedApps", requiredAllowAllTrustedApps ? 1 : 0, RegistryValueKind.DWord);
            //     }
               
            //     if (actualAllowDevelopmentWithoutDevLicense != requiredAllowDevelopmentWithoutDevLicense)
            //     {
            //         subKey.SetValue("AllowDevelopmentWithoutDevLicense", requiredAllowDevelopmentWithoutDevLicense ? 1 : 0, RegistryValueKind.DWord);
            //     }
            // }
            // finally
            // {
            //     if (subKey != null)
            //     {
            //         subKey.Dispose();
            //     }
            // }
        }

        public SideloadingStatus GetStatus()
        {
            using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");

            if (subKey == null)
            {
                return SideloadingStatus.NotAllowed;
            }

            var allowAllTrustedApps = subKey.GetValue("AllowAllTrustedApps");

            if (allowAllTrustedApps == null)
            {
                return SideloadingStatus.NotAllowed;
            }

            var dword1 = allowAllTrustedApps is int dwordValue1 ? dwordValue1 : 0;

            if (dword1 == 0)
            {
                return SideloadingStatus.NotAllowed;
            }

            var allowDevelopmentWithoutDevLicense = subKey.GetValue("AllowDevelopmentWithoutDevLicense");
            if (allowDevelopmentWithoutDevLicense == null)
            {
                return SideloadingStatus.Sideloading;
            }

            var dword2 = allowDevelopmentWithoutDevLicense is int dwordValue2 ? dwordValue2 : 0;
            if (dword2 == 0)
            {
                return SideloadingStatus.Sideloading;
            }

            return SideloadingStatus.DeveloperMode;
        }
    }
}