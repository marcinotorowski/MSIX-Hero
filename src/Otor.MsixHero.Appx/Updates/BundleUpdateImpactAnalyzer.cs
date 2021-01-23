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
    public class BundleUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public Task<UpdateImpactResult> Analyze(string msixBundlePath1, string msixBundlePath2, CancellationToken cancellationToken = default)
        {
            if (msixBundlePath1 == null)
            {
                throw new ArgumentNullException(nameof(msixBundlePath1));
            }

            if (msixBundlePath2 == null)
            {
                throw new ArgumentNullException(nameof(msixBundlePath2));
            }

            if (!IsBundlePackage(msixBundlePath1))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixBundlePath1)} is not a valid MSIX bundle.");
            }

            if (!IsBundlePackage(msixBundlePath2))
            {
                throw new ArgumentException($"File {Path.GetFileName(msixBundlePath2)} is not a valid MSIX bundle.");
            }

            throw new NotSupportedException("Bundles are not supported.");
        }

        private static bool IsBundlePackage(string path)
        {
            try
            {
                using (IAppxFileReader fileReader = new ZipArchiveFileReaderAdapter(path))
                {
                    return fileReader.FileExists("AppxMetadata\\AppxBundleManifest.xml");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}