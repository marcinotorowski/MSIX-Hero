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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Updates
{
    public class MsixUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public async Task<UpdateImpactResults> Analyze(
            string msixPath1, 
            string msixPath2, 
            bool ignoreVersionCheck = false, 
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = default)
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
                throw new ArgumentException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_NotMsix_Format, Path.GetFileName(msixPath1)));
            }

            if (!IsMsixPackage(msixPath2))
            {
                throw new ArgumentException(string.Format(Resources.Localization.Packages_UpdateImpact_Error_NotMsix_Format, Path.GetFileName(msixPath2)));
            }

            progressReporter?.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_ComparingFiles));
            using IAppxFileReader fileReader1 = new ZipArchiveFileReaderAdapter(msixPath1);
            using IAppxFileReader fileReader2 = new ZipArchiveFileReaderAdapter(msixPath2);
            await using var file1 = fileReader1.GetFile("AppxBlockMap.xml");
            await using var file2 = fileReader2.GetFile("AppxBlockMap.xml");
            var reader = new AppxBlockMapUpdateImpactAnalyzer();
            var result = await reader.Analyze(file1, file2, cancellationToken, progressReporter).ConfigureAwait(false);
            result.OldPackage = msixPath1;
            result.NewPackage = msixPath2;
            return result;
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