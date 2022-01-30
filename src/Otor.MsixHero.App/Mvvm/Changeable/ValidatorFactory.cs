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

using System;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ValidatorFactory
    {
        public static Func<string, string> ValidateNotEmptyField(string prompt = null)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
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

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!int.TryParse(value, out _))
                {
                    return prompt == null ? Resources.Localization.Validation_Integer : string.Format(Resources.Localization.Validation_Integer_Named, prompt);
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

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!Uri.TryCreate(value, UriKind.Absolute, out var parsed) || string.IsNullOrEmpty(parsed.Scheme))
                {
                    return prompt == null ? Resources.Localization.Validation_Url : string.Format(Resources.Localization.Validation_Url_Named, prompt);
                }

                switch (parsed.Scheme.ToLowerInvariant())
                {
                    case "http":
                    case "https":
                    case "ftps":
                    case "ftp":
                        return null;
                }

                var protocol = parsed.Scheme + "://";
                return prompt == null ? string.Format(Resources.Localization.Validation_SupportedWebProtocol, protocol) : string.Format(Resources.Localization.Validation_SupportedWebProtocol_Named, prompt, protocol);
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

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!Guid.TryParse(value, out _))
                {
                    return prompt == null ? Resources.Localization.Validation_Guid : string.Format(Resources.Localization.Validation_Guid_Named, prompt);
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

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!Uri.TryCreate(value, UriKind.Absolute, out var parsed))
                {
                    return prompt == null ? Resources.Localization.Validation_Url : string.Format(Resources.Localization.Validation_Url_Named, prompt);
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
                
                var protocol = parsed.Scheme + "://";
                return prompt == null ? string.Format(Resources.Localization.Validation_SupportedWebProtocol, protocol) : string.Format(Resources.Localization.Validation_SupportedWebProtocol_Named, prompt, protocol);
            };
        }

        public static Func<string, string> ValidateVersion(bool required = true, string prompt = null)
        {
            return version =>
            {
                if (string.IsNullOrEmpty(version))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!Version.TryParse(version, out _))
                {
                    return prompt == null ? Resources.Localization.Validation_VersionField : string.Format(Resources.Localization.Validation_VersionField_Named, prompt);
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

                    return prompt == null ? Resources.Localization.Validation_NotEmptyField : string.Format(Resources.Localization.Validation_NotEmptyField_Named, prompt);
                }

                if (!Regex.IsMatch(value, "^[a-fA-F0-9]{64}$"))
                {
                    return prompt == null ? Resources.Localization.Validation_Sha256Field : string.Format(Resources.Localization.Validation_Sha256Field_Named, prompt);
                }

                return null;
            };
        }
    }
}
