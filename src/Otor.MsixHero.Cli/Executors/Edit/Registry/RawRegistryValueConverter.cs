// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Cli.Executors.Edit.Registry
{
    public static class RawRegistryValueConverter
    {
        public static string GetStringFromString(string inputString)
        {
            return inputString;
        }

        public static string[] GetMultiValueFromString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return new[] { inputString };
            }

            return inputString.Split('|');
        }

        public static uint GetDWordFromString(string inputString)
        {
            var longValue = GetQWordFromString(inputString);
            if (longValue > uint.MaxValue)
            {

                throw new ArgumentException($"The value '{inputString}' is not within  the range of DWORD values (0-{uint.MaxValue}).", nameof(inputString));
            }

            return (uint)longValue;
        }

        public static ulong GetQWordFromString(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return 0;
            }

            inputString = inputString.Trim();
            if (inputString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!ulong.TryParse(inputString.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsedHex))
                {
                    throw new ArgumentException($"The value '{inputString}' is not a valid hexadecimal number.", nameof(inputString));
                }

                return parsedHex;
            }

            if (!ulong.TryParse(inputString, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
            {
                throw new ArgumentException($"The value '{inputString}' is not a valid number.", nameof(inputString));
            }

            return parsed;
        }

        public static byte[] GetByteArrayFromString(string inputString)
        {
            inputString = inputString.Trim();

            if (string.IsNullOrEmpty(inputString))
            {
                return Array.Empty<byte>();
            }

            if (inputString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!Regex.IsMatch(inputString, "^0x[a-fA-F0-9]+$", RegexOptions.IgnoreCase))
                {
                    throw new ArgumentException($"The hexadecimal string '{inputString}' could not be parsed.", nameof(inputString));
                }

                return Convert.FromHexString(inputString.Substring(2));
            }

            if (Regex.IsMatch(inputString, @"^[0-9a-fA-f]+\s*(,\s*[0-9a-fA-f]+\s*)*$", RegexOptions.IgnoreCase))
            {
                if (inputString.Split(',').Any(n => !int.TryParse(n, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var _)))
                {
                    throw new ArgumentException($"The comma-separated value '{inputString}' does not represent a correct byte sequence.", nameof(inputString));
                }

                var listOfBytes = new List<byte>();

                foreach (var n in inputString.Split(',').Select(n => n.Trim()))
                {
                    if (n.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if (byte.TryParse(n, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
                        {
                            listOfBytes.Add(parsed);
                        }
                        else
                        {
                            throw new ArgumentException($"The value '{n}' does not represent a correct hex-string for a byte.", nameof(inputString));
                        }
                    }
                    else
                    {
                        if (byte.TryParse(n, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
                        {
                            listOfBytes.Add(parsed);
                        }
                        else
                        {
                            throw new ArgumentException($"The value '{n}' does not represent a correct decimal value for a byte.", nameof(inputString));
                        }
                    }
                }

                return listOfBytes.ToArray();
            }

            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                return Convert.FromBase64String(inputString);
            }
            catch (Exception)
            {
                throw new ArgumentException("The binary value must be a valid base64 string, a comma-separated list of byte values or a hex string representing bytes.", nameof(inputString));
            }
        }

    }
}