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
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.SizeCalculator
{
    public class VhdSizeCalculator : ISizeCalculator
    {
        private static readonly LogSource Logger = new();
        public Task<long> GetRequiredSize(string sourcePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            Logger.Debug().WriteLine($"Determining required size for VHD(X) drive {sourcePath} with extra margin {(int)(100 * extraMargin)}%…");
            const long reserved = 16 * 1024 * 1024;
            const long minSize = 32 * 1024 * 1024;

            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath), Resources.Localization.Packages_Error_EmptyPath);
            }

            long total = 0;

            using (var archive = ZipFile.OpenRead(sourcePath))
            {
                foreach (var item in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    total += item.Length;

                    if (item.Length <= 512)
                    {
                        // Small files are contained in MFT
                        continue;
                    }

                    var surplus = item.Length % 4096;
                    if (surplus != 0)
                    {
                        total += 4096 - surplus;
                    }
                }
            }

            var actualMinSize = Math.Max(minSize, (long)(total * (1 + extraMargin) + reserved));
            Logger.Info().WriteLine("Required minimum size for VHD volume is " + actualMinSize + " bytes.");
            return Task.FromResult(actualMinSize);
        }
    }
}