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
                await console.WriteSuccess(" * Signed by:  " + isTrusted.Trustee);
                await console.WriteSuccess(" * Issuer:     " + isTrusted.Issuer);
                await console.WriteSuccess(" * Thumbprint: " + isTrusted.Thumbprint);
                if (isTrusted.Expires.HasValue)
                {
                    await console.WriteSuccess(" * Expires:    " + isTrusted.Expires.Value);
                }
                else
                {
                    await console.WriteSuccess(" * Expires:    <never>");
                }
            }
            else
            {
                await console.WriteWarning(" * Signed by:  " + isTrusted.Trustee + " (untrusted)");
                await console.WriteWarning(" * Issuer:     " + isTrusted.Issuer);
                await console.WriteWarning(" * Thumbprint: " + isTrusted.Thumbprint);
                if (isTrusted.Expires.HasValue)
                {
                    await console.WriteWarning(" * Expires:    " + isTrusted.Expires.Value);
                }
                else
                {
                    await console.WriteWarning(" * Expires:    <never>");
                }
            }
        }
    }
}
