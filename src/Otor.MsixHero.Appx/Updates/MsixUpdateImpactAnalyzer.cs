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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;

namespace Otor.MsixHero.Appx.Updates
{
    public class MsixUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public async Task<ComparisonResult> Analyze(string msixPath1, string msixPath2, CancellationToken cancellationToken = default)
        {
            if (msixPath1 == null)
            {
                throw new ArgumentNullException(nameof(msixPath1));
            }

            if (msixPath2 == null)
            {
                throw new ArgumentNullException(nameof(msixPath2));
            }

            if (!IsMsixPackage(msixPath1))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixPath1)} is not a valid MSIX.");
            }

            if (!IsMsixPackage(msixPath2))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixPath2)} is not a valid MSIX.");
            }

            using (IAppxFileReader fileReader1 = new ZipArchiveFileReaderAdapter(msixPath1))
            {
                using (IAppxFileReader fileReader2 = new ZipArchiveFileReaderAdapter(msixPath2))
                {
                    await using (var file1 = fileReader1.GetFile("AppxBlockMap.xml"))
                    {
                        await using (var file2 = fileReader2.GetFile("AppxBlockMap.xml"))
                        {
                            var reader = new AppxBlockMapUpdateImpactAnalyzer();
                            return await reader.Analyze(file1, file2, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        private static bool IsMsixPackage(string path)
        {
            try
            {
                using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(path))
                {
                    return fileReader.FileExists("AppxBlockMap.xml");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}