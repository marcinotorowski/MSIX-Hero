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