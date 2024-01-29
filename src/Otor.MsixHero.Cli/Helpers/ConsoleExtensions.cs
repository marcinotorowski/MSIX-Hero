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

using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;

namespace Otor.MsixHero.Cli.Helpers
{
    internal static class ConsoleExtensions
    {
        public static async Task ShowCertSummary(this IConsole console, ISigningManager signingManager, string certificateOrSignedFile)
        {
            var isTrusted = await signingManager.IsTrusted(certificateOrSignedFile).ConfigureAwait(false);
            if (isTrusted.Trustee == null)
            {
                return;
            }

            if (isTrusted.IsTrusted)
            {
                await console.WriteSuccess(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_SignedBy_Aligned_Format, isTrusted.Trustee));
                await console.WriteSuccess(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Issuer_Aligned_Format, isTrusted.Issuer));
                await console.WriteSuccess(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Thumbprint_Aligned_Format, isTrusted.Thumbprint));
                if (isTrusted.Expires.HasValue)
                {
                    await console.WriteSuccess(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Expires_Aligned_Format, isTrusted.Expires.Value));
                }
                else
                {
                    await console.WriteSuccess(" * " + Resources.Localization.CLI_CertificateSummary_ExpiresNever_Aligned);
                }
            }
            else
            {
                await console.WriteWarning(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_SignedByUntrusted_Aligned_Format, isTrusted.Trustee));
                await console.WriteWarning(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Issuer_Aligned_Format, isTrusted.Issuer));
                await console.WriteWarning(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Thumbprint_Aligned_Format, isTrusted.Thumbprint));
                if (isTrusted.Expires.HasValue)
                {
                    await console.WriteWarning(" * " + string.Format(Resources.Localization.CLI_CertificateSummary_Expires_Aligned_Format, isTrusted.Expires.Value));
                }
                else
                {
                    await console.WriteWarning(" * " + Resources.Localization.CLI_CertificateSummary_ExpiresNever_Aligned);
                }
            }
        }
    }
}
