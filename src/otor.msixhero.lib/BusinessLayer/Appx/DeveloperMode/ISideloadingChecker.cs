using System;
using Microsoft.Win32;

namespace otor.msixhero.lib.BusinessLayer.Appx.DeveloperMode
{
    public enum SideloadingStatus
    {
        NotAllowed = 1,

        Sideloading = 2,

        DeveloperMode = 3
    }

    public interface ISideloadingChecker
    {
        SideloadingStatus GetStatus();

        void SetStatus(SideloadingStatus status);
    }

    public class RegistrySideloadingChecker : ISideloadingChecker
    {
        public void SetStatus(SideloadingStatus status)
        {
            using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            RegistryKey subKey = null;
            try
            {
                subKey = key.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
                if (subKey == null)
                {
                    subKey = key.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock");
                    if (subKey == null)
                    {
                        throw new InvalidOperationException("Could not create Registry Key to enable sideloading.");
                    }
                }

                var regAllowAllTrustedApps = subKey.GetValue("AllowAllTrustedApps");
                var regAllowDevelopmentWithoutDevLicense = subKey.GetValue("AllowDevelopmentWithoutDevLicense");

                var requiredAllowAllTrustedApps = status >= SideloadingStatus.Sideloading;
                var requiredAllowDevelopmentWithoutDevLicense = status >= SideloadingStatus.DeveloperMode;
                var actualAllowAllTrustedApps = regAllowAllTrustedApps != null && regAllowAllTrustedApps is int value1 && value1 > 0;
                var actualAllowDevelopmentWithoutDevLicense = regAllowDevelopmentWithoutDevLicense != null && regAllowDevelopmentWithoutDevLicense is int value2 && value2 > 0;

                if (actualAllowAllTrustedApps != requiredAllowAllTrustedApps)
                {
                    subKey.SetValue("AllowAllTrustedApps", requiredAllowAllTrustedApps ? 1 : 0, RegistryValueKind.DWord);
                }

                if (actualAllowDevelopmentWithoutDevLicense != requiredAllowDevelopmentWithoutDevLicense)
                {
                    subKey.SetValue("AllowDevelopmentWithoutDevLicense", requiredAllowDevelopmentWithoutDevLicense ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            finally
            {
                if (subKey != null)
                {
                    subKey.Dispose();
                }
            }
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
