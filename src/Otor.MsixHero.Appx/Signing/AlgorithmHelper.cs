using System.Text.RegularExpressions;

namespace Otor.MsixHero.Appx.Signing
{
    internal static class AlgorithmHelper
    {
        /// <summary>
        /// Tries to determine the signing algorithm (like "sha256") from a certificate signature algorithm friendly name or OID.
        /// Returns normalized string like "sha256" or null when it cannot be determined.
        /// </summary>
        public static string GetAlgorithmTypeFromFriendlyName(string friendlyName)
        {
            if (string.IsNullOrWhiteSpace(friendlyName))
            {
                return null;
            }

            var lower = friendlyName.ToLowerInvariant();

            // Try to find sha<digits> in many common formats: sha256, sha-256, sha_256, sha256withrsa, ecdsa-with-sha256, etc.
            var regexMatch = Regex.Match(lower, @"(?<alg>sha[\-_]?\d+)", RegexOptions.IgnoreCase);
            if (regexMatch.Success)
            {
                // normalize to e.g. sha256
                return regexMatch.Groups["alg"].Value.Replace("-", string.Empty).Replace("_", string.Empty);
            }

            // try patterns like 'withsha256' or 'with-sha256' or 'ecdsa-with-sha256'
            regexMatch = Regex.Match(lower, @"with[-_]?sha[-_]?([0-9]+)", RegexOptions.IgnoreCase);
            if (regexMatch.Success)
            {
                return "sha" + regexMatch.Groups[1].Value;
            }

            // try 'sha256' or similar
            regexMatch = Regex.Match(lower, @"sha\s*([0-9]+)", RegexOptions.IgnoreCase);
            if (regexMatch.Success)
            {
                return "sha" + regexMatch.Groups[1].Value;
            }

            // Common OID fallbacks
            switch (lower)
            {
                case "1.2.840.10045.4.3.2": // ecdsa-with-SHA256
                case "1.2.840.113549.1.1.11": // sha256WithRSAEncryption
                    return "sha256";
            }

            return null;
        }
    }
}
