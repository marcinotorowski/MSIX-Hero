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
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Infrastructure.Localization;

public class FileScanTranslationProvider : ITranslationProvider
{
    private readonly DirectoryInfo baseDirectory;

    public FileScanTranslationProvider()
    {
        baseDirectory = new FileInfo(typeof(FileScanTranslationProvider).Assembly.Location).Directory;
    }

    private FileScanTranslationProvider(DirectoryInfo baseDirectory)
    {
        this.baseDirectory = baseDirectory;
    }

    public IEnumerable<CultureInfo> GetAvailableTranslations()
    {
        yield return CultureInfo.GetCultureInfo("en"); // standard language (English)

        foreach (var dir in this.baseDirectory.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            if (string.Equals("en", dir.Name, StringComparison.OrdinalIgnoreCase) || !Regex.IsMatch(dir.Name, "^[a-zA-Z]{2}(?:-[a-zA-Z]{2,})?$"))
            {
                continue;
            }

            // Check if any of these contains at least one MSIX-Hero satellite resource…
            var anyFile = dir.EnumerateFiles("*MsixHero*.Resources.dll").Any();
            if (anyFile)
            {
                CultureInfo ci;
                try
                {
                    ci = CultureInfo.GetCultureInfo(dir.Name);
                }
                catch
                {
                    continue;
                }

                yield return ci;
            }
        }
    }
}