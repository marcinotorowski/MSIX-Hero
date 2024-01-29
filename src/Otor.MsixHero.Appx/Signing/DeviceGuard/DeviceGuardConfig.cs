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

namespace Otor.MsixHero.Appx.Signing.DeviceGuard
{
    public class DeviceGuardConfig
    {
        public DeviceGuardConfig(string accessToken, string refreshToken, string subject)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            this.Subject = subject;
        }

        public DeviceGuardConfig(string accessToken, string refreshToken)
        {
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
        }

        public DeviceGuardConfig()
        {
        }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public string Subject { get; set; }
    }
}
