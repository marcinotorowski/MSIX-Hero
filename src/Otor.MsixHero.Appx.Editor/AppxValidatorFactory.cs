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
using System.Linq;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Appx.Editor
{
    public static class AppxValidatorFactory
    {
        public static Func<string, string> ValidateResourceId(bool required = true)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return "The resource ID cannot be empty.";
                }

                if (value.Length < 1 || value.Length > 30)
                {
                    return "The resource ID must be a string between 1 and 30 characters in length.";
                }

                if (!value.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-'))
                {
                    return "The resource ID must consist of alpha-numeric, period and dash characters.";
                }

                return ValidateNameAndResourceIdSpecialKeywords(value);
            };
        }

        public static Func<string, string> ValidatePackageName(bool required = true)
        {
            return value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return "The package name cannot be empty.";
                }

                if (value.Length < 3 || value.Length > 50)
                {
                    return "The package name must be a string between 3 and 50 characters in length.";
                }

                if (!value.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-'))
                {
                    return "The package name must consist of alpha-numeric, period and dash characters.";
                }
                
                return ValidateNameAndResourceIdSpecialKeywords(value);
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

                    return "The publisher cannot be empty.";
                }

                if (!Regex.IsMatch(value.Replace(", ", ","), @"^(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)\s*=\s*(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*"")(?:\+(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)=(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*""))*(?:,(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)=(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*"")(?:\+(?:[A-Za-z][\w-]*|\d+(?:\.\d+)*)\s*=\s*(?:#(?:[\dA-Fa-f]{2})+|(?:[^,=\+<>#;\\""]|\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*|""(?:[^\\""] |\\[,=\+<>#;\\""]|\\[\dA-Fa-f]{2})*""))*)*$"))
                {
                    // todo: Some better validation, RFC compliant (https://docs.microsoft.com/en-us/windows/win32/api/wincrypt/nf-wincrypt-certnametostra)
                    return "Publisher name must be a valid DN string (for example CN=Author)";
                }

                return null;
            };
        }
        
        public static Func<string, string> ValidateVersion(bool required = true)
        {
            return version =>
            {
                if (string.IsNullOrEmpty(version))
                {
                    if (!required)
                    {
                        return null;
                    }

                    return "The version cannot be empty.";
                }

                if (!Version.TryParse(version, out var v) || v.Major == -1 || v.Minor == -1 || v.Revision == -1 || v.Build == -1)
                {
                    return "The version must be in format #.#.#.#.";
                }

                return null;
            };
        }
        
        private static string ValidateNameAndResourceIdSpecialKeywords(string value)
        {
            switch (value)
            {
                case ".":
                case "..":
                case "con":
                case "prn":
                case "aux":
                case "nul":
                case "com1":
                case "com2":
                case "com3":
                case "com4":
                case "com5":
                case "com6":
                case "com7":
                case "com8":
                case "com9":
                case "lpt1":
                case "lpt2":
                case "lpt3":
                case "lpt4":
                case "lpt5":
                case "lpt6":
                case "lpt7":
                case "lpt8":
                case "lpt9":
                    return "The value cannot be equal to one of the restricted keywords.";
            }

            if (value.Length > 3)
            {
                switch (value.Substring(0, 4))
                {
                    case "con.":
                    case "prn.":
                    case "aux.":
                    case "nul.":
                    case "xn--":
                        return "The value cannot start with a restricted prefix '" + value.Substring(0, 4) + "'.";
                }

                if (value.Length > 4)
                {
                    switch (value.Substring(0, 5))
                    {
                        case "com1.":
                        case "com2.":
                        case "com3.":
                        case "com4.":
                        case "com5.":
                        case "com6.":
                        case "com7.":
                        case "com8.":
                        case "com9.":
                        case "lpt1.":
                        case "lpt2.":
                        case "lpt3.":
                        case "lpt4.":
                        case "lpt5.":
                        case "lpt6.":
                        case "lpt7.":
                        case "lpt8.":
                        case "lpt9.":
                            return "The value cannot start with a restricted prefix '" + value.Substring(0, 5) + "'.";
                    }
                }
            }

            if (value.EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                return "The value cannot end with dot.";
            }

            if (value.Contains(".xn--", StringComparison.OrdinalIgnoreCase))
            {
                return "The value cannot contain restricted string '.xn--'.";
            }

            return null;
        }
    }
}
