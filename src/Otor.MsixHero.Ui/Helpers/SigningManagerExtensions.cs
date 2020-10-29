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

namespace Otor.MsixHero.Ui.Helpers
{
    public static class SigningManagerExtensions
    {
        public static Task SignPackageWithDeviceGuard(
            this ISigningManager signingManager,
            string package,
            DeviceGuardConfiguration configuration,
            string timestampUrl = null,
            IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = default)
        {
            var tokens = new DeviceGuardConfig();

            var crypto = new Crypto();
            tokens.AccessToken = FromSecureString(crypto.Unprotect(configuration.EncodedAccessToken));
            tokens.RefreshToken = FromSecureString(crypto.Unprotect(configuration.EncodedRefreshToken));
            tokens.Subject = configuration.Subject;
            
            var token = new DgssTokenCreator().CreateDeviceGuardJsonToken(tokens, cancellationToken);
            return signingManager.SignPackageWithDeviceGuard(package, configuration.UseV1, token, timestampUrl, increaseVersion, cancellationToken, progress);
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
