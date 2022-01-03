// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public static class UserHelper
    {
        private static bool? isLocalAdmin;
        public static bool IsAdministrator()
        {
            if (isLocalAdmin.HasValue)
            {
                return isLocalAdmin.Value;
            }

            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            isLocalAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            return isLocalAdmin.Value;
        }

        public static Task<bool> IsAdministratorAsync(CancellationToken cancellationToken)
        {
            if (isLocalAdmin.HasValue)
            {
                return Task.FromResult(isLocalAdmin.Value);
            }

            return Task.Run(() =>
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                isLocalAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return isLocalAdmin.Value;
            },
            cancellationToken);
        }
    }
}
