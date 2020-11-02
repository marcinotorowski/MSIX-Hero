using System;
using System.Runtime.InteropServices;
using System.Security;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;

namespace Otor.MsixHero.Cli.Helpers
{
    public static class DeviceGuardConfigurationExtensions
    {
        public static DeviceGuardConfig FromConfiguration(this DeviceGuardConfiguration configuration)
        {
            var tokens = new DeviceGuardConfig();

            var crypto = new Crypto();
            tokens.AccessToken = FromSecureString(crypto.Unprotect(configuration.EncodedAccessToken));
            tokens.RefreshToken = FromSecureString(crypto.Unprotect(configuration.EncodedRefreshToken));
            tokens.Subject = configuration.Subject;

            return tokens;
        }

        private static string FromSecureString(SecureString secureString)
        {
            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                // ReSharper disable once AssignNullToNotNullAttribute
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                if (unmanagedString != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }
        }
    }
}
