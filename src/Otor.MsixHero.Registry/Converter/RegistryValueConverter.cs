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

using System.Linq;
using Otor.MsixHero.Registry.Tokenizer;

namespace Otor.MsixHero.Registry.Converter
{
    public static class RegistryValueConverter
    {
        private static readonly RegistryTokenizer Tokenizer = new RegistryTokenizer();

        public static string ToMsixFormat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.TrimStart().StartsWith('"'))
            {
                var findClosing = value.IndexOf('"', value.IndexOf('"') + 1);
                if (findClosing == -1)
                {
                    return value;
                }

                return value.Substring(0, value.IndexOf('"') + 1) + Tokenizer.Tokenize(value) + value.Substring(findClosing);
            }

            var findSpace = value.IndexOf(' ');
            if (findSpace == -1)
            {
                return Tokenizer.Tokenize(value);
            }

            return Tokenizer.Tokenize(value.Substring(0, findSpace)) + value.Remove(0, findSpace);
        }

        public static string[] ToMsixFormat(string[] value)
        {
            if (value == null || !value.Any())
            {
                return value;
            }

            return value.Select(ToMsixFormat).ToArray();
        }

    }
}