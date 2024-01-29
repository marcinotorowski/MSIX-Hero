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
