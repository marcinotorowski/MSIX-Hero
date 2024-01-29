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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Registry.Parser
{
    public class RegFileParser
    {
        private const string HeaderWindowsRegEdit4 = "REGEDIT4";
        private const string HeaderWindowsRegEdit = "Windows Registry Editor";

        public IList<RegistryEntry> Parse(string sourceFile)
        {
            var result = new List<RegistryEntry>();
            var lines = GetLines(sourceFile);

            if (!lines.Any())
            {
                return result;
            }

            var winReg = lines.First().Content.Contains(HeaderWindowsRegEdit4) ? 4 : 5;

            RegistryRoot? currentRoot = null;
            string currentKey = null;

            foreach (var line in lines)
            {
                if (line.Content.StartsWith("[", StringComparison.Ordinal))
                {
                    var keyTemp = line.Content.TrimStart('[');
                    if (string.IsNullOrEmpty(keyTemp) || string.IsNullOrWhiteSpace(keyTemp))
                    {
                        continue;
                    }

                    var key = keyTemp;
                    if (keyTemp.EndsWith("]", StringComparison.Ordinal))
                    {
                        key = keyTemp.Remove(keyTemp.Length - 1, 1);
                    }

                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    key = key.TrimEnd('\\');

                    var parts = key.Split('\\');

                    currentRoot = Enum.Parse<RegistryRoot>(parts[0], true);

                    if (key.Contains("\\"))
                    {
                        currentKey = key.Substring(key.IndexOf('\\')).TrimStart('\\');
                    }

                    continue;
                }

                if (currentRoot == null)
                {
                    throw new FormatException(string.Format(Resources.Localization.Registry_Error_NoRoot_Format, line));
                }

                if (string.IsNullOrEmpty(currentKey))
                {
                    throw new FormatException(string.Format(Resources.Localization.Registry_Error_EmptyKeyPath_Format, line));
                }

                var entry = new RegistryEntry
                {
                    Root = (RegistryRoot) currentRoot,
                    Key = currentKey
                };

                var split = line.Content.Split('=');

                if (line.Content.StartsWith("@=", StringComparison.Ordinal))
                {
                    entry.Name = null;
                    entry.Type = ValueType.Default;
                    entry.Value = line.Content.Substring(2).Trim('"');
                }
                else if (split.Length == 2)
                {
                    entry.Name = GetName(split[0]);
                    entry.Value = GetValueAndType(winReg, line, split[1], out entry.Type);

                    if (entry.Name == null)
                    {
                        entry.Type = ValueType.Default;
                    }
                }
                else
                {
                    var match = Regex.Match(line.Content, "\"(.*)\"=(?(\".*\")\"(.*)\"|(.*))");

                    if (match.Groups.Count != 4)
                    {
                        throw new FormatException(string.Format(Resources.Localization.Registry_Error_InvalidNameContentPair_Format, line));
                    }

                    entry.Type = ValueType.String;
                    entry.Name = match.Groups[1].ToString().Trim();

                    var entryValue = string.IsNullOrEmpty(match.Groups[2].ToString()) ? match.Groups[3].ToString() : match.Groups[2].ToString();

                    if (entryValue.IndexOf('\\') != -1)
                    {
                        var stringBuilder = new StringBuilder();
                        for (var i = 0; i < entryValue.Length; i++)
                        {
                            var character = entryValue[i];
                            if (character != '\\')
                            {
                                stringBuilder.Append(character);
                                continue;
                            }

                            if (i + 1 == entryValue.Length)
                            {
                                continue;
                            }

                            stringBuilder.Append(entryValue[i + 1]);
                        }

                        entry.Value = stringBuilder.ToString();
                    }

                    entry.Value = entryValue;
                }

                result.Add(entry);
            }

            return result;
        }

        private static object GetValueAndType(int winRegVersion, Line line, string typeValuePair, out ValueType type)
        {
            if (typeValuePair == null)
            {
                throw new ArgumentNullException(nameof(typeValuePair));
            }

            typeValuePair = typeValuePair.Trim();

            // no content
            if (typeValuePair == string.Empty)
            {
                type = ValueType.Default;
                return null;
            }

            // string content
            if (typeValuePair.StartsWith('"'))
            {
                if (!typeValuePair.EndsWith('"'))
                {
                    throw new FormatException(string.Format(Resources.Localization.Registry_Error_NoEndingQuote_Format, line));
                }

                typeValuePair = typeValuePair.Trim('"');

                if (typeValuePair.IndexOf('\\') != -1)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < typeValuePair.Length; i++)
                    {
                        var character = typeValuePair[i];
                        if (character != '\\')
                        {
                            stringBuilder.Append(character);
                            continue;
                        }

                        i++;

                        if (i == typeValuePair.Length)
                        {
                            continue;
                        }

                        stringBuilder.Append(typeValuePair[i]);
                    }

                    typeValuePair = stringBuilder.ToString();
                }

                type = ValueType.String;
                return typeValuePair;
            }

            var parts = typeValuePair.Split(':');

            // check type:value pair
            if (parts.Length != 2)
            {
                throw new FormatException(string.Format(Resources.Localization.Registry_Error_InvalidTypeValuePair_Format, line));
            }

            if (string.IsNullOrWhiteSpace(parts[0]))
            {
                throw new FormatException(string.Format(Resources.Localization.Registry_Error_TypeEmpty_Format, line));
            }

            var valueStr = parts[1].Trim();
            var typeStr = parts[0].Trim().ToLower();

            switch (typeStr)
            {
                // Dword
                case "dword":
                {
                    if (!string.IsNullOrEmpty(valueStr))
                        try
                        {
                            type = ValueType.DWord;
                            return Convert.ToUInt32(valueStr, 16);
                        }
                        catch (OverflowException)
                        {
                            throw new FormatException(string.Format(Resources.Localization.Registry_Error_TooBigDwordNumber_Format, line));
                        }

                    break;
                }

                // Binary data (comma-delimited list of hexadecimal values)
                case "hex":
                    type = ValueType.Binary;
                    var bytes = GetStringFromCommaSeparated(valueStr);
                    return Enumerable.Range(0, bytes.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(bytes.Substring(x, 2), 16)).ToArray();

                // REG_NONE (comma-delimited list of hexadecimal values)
                case "hex(0)":
                    type = ValueType.None;
                    return GetStringFromCommaSeparated(valueStr, ' ');

                // String (comma-delimited list of hexadecimal values representing a UTF-16LE null-terminated string)
                case "hex(1)":
                    type = ValueType.String;
                    return UnicodeHexToString(valueStr, winRegVersion == 4 ? 2 : 4);

                // Expandable string value data (comma-delimited list of hexadecimal values)
                case "hex(2)":
                    type = ValueType.Expandable;
                    return UnicodeHexToString(valueStr, winRegVersion == 4 ? 2 : 4);

                // DWORD value (as comma-delimited list of 4 hexadecimal values, in little-indian byte order)
                case "hex(4)":
                    type = ValueType.DWord;
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        if (valueStr.Split(',').Length != 4)
                        {
                            throw new FormatException(string.Format(Resources.Localization.Registry_Error_WrongHex4_Format, line));
                        }

                        return Convert.ToInt32(GetStringFromCommaSeparated(valueStr, null, true), 16);
                    }

                    break;

                // DWORD value (comma-delimited list of 4 hexadecimal values, in big-indian byte order)
                case "hex(5)":
                    type = ValueType.DWordBigEndian;
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        if (valueStr.Split(',').Length != 4)
                        {
                            throw new FormatException(string.Format(Resources.Localization.Registry_Error_WrongHex5_Format, line));
                        }

                        return Convert.ToInt32(GetStringFromCommaSeparated(valueStr), 16).ToString(CultureInfo.InvariantCulture);
                    }

                    break;

                // Multi-string (comma separated list of hexadecimal values separated by two null byte ending with four null bytes)
                case "hex(7)":
                    type = ValueType.Multi;
                    var replacements = winRegVersion == 4 ? new Dictionary<string, string> {{"00", "\n" } } : new Dictionary<string, string> {{"0000", "\n" } };
                    return UnicodeHexToString(valueStr, winRegVersion == 4 ? 2 : 4, replacements).Split("\n", StringSplitOptions.RemoveEmptyEntries);

                // ReSharper disable once CommentTypo
                // QWORD value (comma-delimited list of 8 hexadecimal values, in little-indian byte order)
                case "hex(b)":
                    type = ValueType.QWord;
                    if (!string.IsNullOrEmpty(valueStr))
                    {
                        try
                        {
                            return Convert.ToUInt64(GetStringFromCommaSeparated(valueStr, null, true), 16);
                        }
                        catch (OverflowException)
                        {
                            // ReSharper disable once StringLiteralTypo
                            throw new FormatException(string.Format(Resources.Localization.Registry_Error_TooBigQwordNumber_Format, line));
                        }
                    }

                    break;

                default:
                    throw new FormatException(string.Format(Resources.Localization.Registry_Error_UnknownType_Format, line));
            }

            type = ValueType.None;
            return null;
        }

        private static string GetStringFromCommaSeparated(string fullString, char? separator = null, bool reverse = false)
        {
            if (fullString == null)
            {
                throw new ArgumentNullException(nameof(fullString));
            }

            IEnumerable<string> parts = fullString.Split(',');

            if (reverse)
            {
                parts = parts.Reverse();
            }

            if (separator != null)
            {
                return string.Join(separator.Value, parts.Select(p => p.Trim()));
            }

            return string.Concat(parts.Select(p => p.Trim()));
        }

        private static string GetName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            name = name.Trim();

            if (string.Equals(name, "@", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.IsNullOrEmpty(name) || !name.StartsWith('"') || !name.EndsWith('"'))
            {
                throw new FormatException(string.Format(Resources.Localization.Registry_Error_NameParsingFailed_Format, name));
            }

            return name.Trim('"').Trim();
        }

        private static List<Line> GetLines(string filename)
        {
            var lines = File.ReadAllLines(filename);
            var result = new List<Line>();

            for (var i = 0; i < lines.Length; i++)
            {
                var currentLine = lines[i].Trim();

                // skip empty lines and comments
                if (string.IsNullOrEmpty(currentLine) || currentLine.StartsWith(";"))
                {
                    continue;
                }

                // skip header created by windows registry editor on export
                if (currentLine.StartsWith(HeaderWindowsRegEdit) || currentLine.StartsWith(HeaderWindowsRegEdit4))
                {
                    continue;
                }

                // check valid
                if (!currentLine.StartsWith("@", StringComparison.OrdinalIgnoreCase) &&
                    !currentLine.StartsWith("\"", StringComparison.Ordinal) &&
                    !(currentLine.StartsWith("[", StringComparison.Ordinal) &&
                      currentLine.EndsWith("]", StringComparison.Ordinal)))
                {
                    throw new FormatException(string.Format(Resources.Localization.Registry_Error_LineParsingFailed_Format, i + 1, lines[i]));
                }

                // concatenate lines
                while (currentLine.EndsWith('\\'))
                {
                    currentLine = currentLine.TrimEnd('\\');
                    if (i + 1 < lines.Length)
                    {
                        currentLine += lines[i + 1].Trim();
                        lines[i + 1] = "";
                    }
                    else
                    {
                        throw new FormatException(string.Format(Resources.Localization.Registry_Error_Eol_Format, i + 1));
                    }

                    i++;
                }

                result.Add(new Line(i + 1, currentLine.Trim()));
            }

            return result;
        }

        private static string UnicodeHexToString(string hexString, int charLength, IDictionary<string, string> replacements = null)
        {
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            hexString = Regex.Replace(hexString, "[^a-fA-F0-9]", string.Empty);
            hexString = hexString.Substring(0, hexString.Length - charLength);

            if (hexString.Length % charLength != 0)
            {
                hexString = hexString.PadLeft(hexString.Length + charLength - hexString.Length % charLength, '0');
            }

            var stringBuilder = new StringBuilder();

            for (var i = 0; i < hexString.Length; i += charLength)
            {
                var currentValue = hexString.Substring(i, charLength);

                if (charLength == 4)
                {
                    currentValue = currentValue.Substring(2, 2) + currentValue.Substring(0, 2);
                }

                if (replacements != null)
                {
                    if (replacements.TryGetValue(currentValue, out var temp))
                    {
                        stringBuilder.Append(temp);
                        continue;
                    }
                }

                stringBuilder.Append((char) short.Parse(currentValue, NumberStyles.HexNumber));
            }

            return new string(stringBuilder.ToString().ToArray());
        }
    }
}