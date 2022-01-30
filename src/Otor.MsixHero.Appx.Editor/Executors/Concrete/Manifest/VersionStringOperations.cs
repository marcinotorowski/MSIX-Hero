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

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest
{
    public static class VersionStringOperations
    {
        public static string ResolveMaskedVersion(string newValueWithMask, string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue))
            {
                return ResolveMaskedVersion(newValueWithMask);
            }

            if (Version.TryParse(currentValue, out var parsed))
            {
                return ResolveMaskedVersion(newValueWithMask, parsed);
            }

            throw new ArgumentException(string.Format(Resources.Localization.AppxEditor_Error_NotAVersion_Format, currentValue));
        }

        public static string ResolveMaskedVersion(string newValueWithMask, Version currentValue = null)
        {
            if (newValueWithMask == null)
            {
                return currentValue?.ToString() ?? "1.0.0.0";
            }

            if (Version.TryParse(newValueWithMask, out var parsedValue))
            {
                // this means the user passed something which looks like a version
                // but let's not trust him too much and make sure it's a four-unit string:
                return $"{Math.Max(parsedValue.Major, 0)}.{Math.Max(parsedValue.Minor, 0)}.{Math.Max(parsedValue.Build, 0)}.{Math.Max(parsedValue.Revision, 0)}";
            }

            // otherwise if the user set the value to 'auto' we change his input to:
            // *.*.*.+
            if (string.Equals(newValueWithMask, "auto", StringComparison.OrdinalIgnoreCase))
            {
                if (currentValue == null)
                {
                    return "1.0.0.0";
                }

                newValueWithMask = "*.*.*.+";
            }

            // now we apply some special logic, where:
            // *, x or empty value means take the current unit
            // + or ^ means increase the version by on1

            var split = newValueWithMask.Split('.');
            if (split.Length > 4)
            {
                throw new ArgumentException(string.Format(Resources.Localization.AppxEditor_Error_NotAVersion_Format, newValueWithMask));
            }

            if (currentValue == null)
            {
                currentValue = new Version(1, 0, 0, 0);
            }
            
            var versionString = string.Empty;
            for (var i = 0; i < 4; i++)
            {
                if (i > 0)
                {
                    versionString += ".";
                }

                int currentUnit;

                switch (i)
                {
                    case 0:
                        currentUnit = Math.Max(0, currentValue.Major);
                        break;
                    case 1:
                        currentUnit = Math.Max(0, currentValue.Minor);
                        break;
                    case 2:
                        currentUnit = Math.Max(0, currentValue.Build);
                        break;
                    case 3:
                        currentUnit = Math.Max(0, currentValue.Revision);
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (i < split.Length)
                {
                    switch (split[i])
                    {
                        case "+":
                        case "^":
                            versionString += (currentUnit + 1).ToString();
                            break;
                        case "x":
                        case "*":
                        case "":
                            versionString += currentUnit.ToString();
                            break;
                        default:
                            if (!int.TryParse(split[i], out _))
                            {
                                throw new ArgumentException(string.Format(Resources.Localization.AppxEditor_Error_NotAVersion_Format, newValueWithMask));
                            }

                            versionString += split[i];
                            break;
                    }
                }
                else
                {
                    versionString += "0";
                }
            }

            if (!Version.TryParse(versionString, out var _))
            {
                throw new ArgumentException(string.Format(Resources.Localization.AppxEditor_Error_NotAVersion_Format, versionString));
            }

            return versionString;
        }
    }
}