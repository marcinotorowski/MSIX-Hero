using System;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Ui.Domain
{
    public class ValidatorFactory
    {
        public static Func<string, string> ValidateNotEmptyField(string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                return null;
            };
        }


        public static Func<string, string> ValidateSubject(bool required = true)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return "The publisher may not be empty.";
                }

                if (!Regex.IsMatch(value.Replace(", ", ","), @"^(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)\s*=\s*(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*"")(?:\+(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)=(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*""))*(?:,(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)=(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*"")(?:\+(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)\s*=\s*(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*""))*)*$"))
                {
                    // todo: Some better validation, RFC compliant
                    return "Publisher name must be a valid DN string (for example CN=Author)";
                }

                return null;
            };
        }

        public static Func<string, string> ValidateInteger(bool required = false, string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                if (!int.TryParse(value, out _))
                {
                    return prompt == null ? "This value must be an integer" : $"{prompt} is not an integer";
                }

                return null;
            };
        }

        public static Func<string, string> ValidateUrl(bool required, string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                if (!Uri.TryCreate(value, UriKind.Absolute, out var parsed) || string.IsNullOrEmpty(parsed.Scheme))
                {
                    return prompt == null ? "This value is not a valid URL." : $"{prompt} is not a valid URL.";
                }

                switch (parsed.Scheme.ToLowerInvariant())
                {
                    case "http":
                    case "https":
                    case "ftps":
                    case "ftp":
                        return null;
                }

                return prompt == null ? $"Protocol {parsed.Scheme}:// is not supported." : $"{prompt} has an unsupported protocol {parsed.Scheme}://.";
            };
        }

        public static Func<string, string> ValidateGuid(bool required, string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }
                    
                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                if (!Guid.TryParse(value, out _))
                {
                    return prompt == null ? "This value is not a valid URL." : $"{prompt} is not a valid URL.";
                }

                return null;
            };
        }

        public static Func<string, string> ValidateUri(bool required, string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                if (!Uri.TryCreate(value, UriKind.Absolute, out var parsed))
                {
                    return prompt == null ? "This value is not a valid URI." : $"{prompt} is not a valid URL.";
                }

                switch (parsed.Scheme.ToLowerInvariant())
                {
                    case "http":
                    case "https":
                    case "ftps":
                    case "ftp":
                    case "file":
                        return null;
                }

                return prompt == null
                    ? $"Protocol {parsed.Scheme}:// is not supported."
                    : $"{prompt} has an unsupported protocol {parsed.Scheme}://.";
            };
        }

        public static Func<string, string> ValidateVersion(bool required, string prompt = null)
        {
            return version =>
            {
                if (string.IsNullOrEmpty(version))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? "The version may not be empty."  : $"{prompt} may not be empty.";
                }

                if (!Version.TryParse(version, out _))
                {
                    return prompt == null ? "The version must be in format #.#.#.#." : $"{prompt} must be in format #.#.#.#.";
                }

                return null;
            };
        }

        public static Func<string, string> ValidateSha256(bool required, string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? "This value may not be empty." : $"{prompt} may not be empty.";
                }

                if (!Regex.IsMatch(value, "^[a-fA-F0-9]{64}$"))
                {
                    return prompt == null ? "SHA-256 string is invalid. It must consist of exactly 64 hexadecimal characters." : "{prompt} has an invalid value. It must ba a valid SHA-256 hash consisting of exactly 64 hexadecimal characters.";
                }

                return null;
            };
        }
    }
}
