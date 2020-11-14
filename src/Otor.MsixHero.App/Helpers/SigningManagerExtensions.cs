using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Helpers
{
    public static class SigningManagerExtensions
    {
        public static Task SignPackageWithDeviceGuardFromUi(
            this ISigningManager signingManager,
            string package,
            DeviceGuardConfiguration configuration,
            string timestampUrl = null,
            IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            var tokens = configuration.FromConfiguration();
            return signingManager.SignPackageWithDeviceGuard(package, true, tokens, configuration.UseV1, timestampUrl, increaseVersion, cancellationToken, progress);
        }

        private static DeviceGuardConfig FromConfiguration(this DeviceGuardConfiguration configuration)
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