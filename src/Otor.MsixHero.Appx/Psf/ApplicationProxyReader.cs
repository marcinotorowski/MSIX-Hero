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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.Appx.Psf
{
    public abstract class ApplicationProxyReader
    {
        public abstract Task<BaseApplicationProxy> Inspect(CancellationToken cancellationToken = default);

        public static ApplicationProxyReader Create(string applicationId, string originalEntryPoint, IAppxFileReader fileReader)
        {
            if (string.Equals(originalEntryPoint, @"AI_STUBS\AiStub.exe", StringComparison.OrdinalIgnoreCase))
            {
                if (fileReader.FileExists(@"AI_STUBS\AiStub.exe"))
                {
                    return new AdvancedInstallerProxyReader(applicationId, @"AI_STUBS\AiStub.exe", fileReader);
                }
            }
            else if (string.Equals(originalEntryPoint, @"AI_STUBS\AiStubElevated.exe", StringComparison.OrdinalIgnoreCase))
            {
                if (fileReader.FileExists(@"AI_STUBS\AiStubElevated.exe"))
                {
                    return new AdvancedInstallerProxyReader(applicationId, @"AI_STUBS\AiStubElevated.exe", fileReader);
                }
            }
            else
            {
                var fileName = Path.GetFileName(originalEntryPoint);

                if (string.Equals(fileName, "msixhelper32.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return new MsixHelperProxyReader(applicationId, originalEntryPoint, fileReader);
                }

                if (string.Equals(fileName, "msixhelper64.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return new MsixHelperProxyReader(applicationId, originalEntryPoint, fileReader);
                }
                
                if (fileName.Contains("psf", StringComparison.OrdinalIgnoreCase) && string.Equals(".exe", Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase))
                {
                    return new PackageSupportFrameworkProxyReader(applicationId, originalEntryPoint, fileReader);
                }
            }

            return null;
        }

        public static ApplicationProxyReader Create(string applicationId, string originalEntryPoint, string packageRootFolder)
        {
            if (!Directory.Exists(packageRootFolder))
            {
                throw new ArgumentException("Package folder does not exist.");
            }

            using IAppxFileReader fileReader = new DirectoryInfoFileReaderAdapter(new DirectoryInfo(packageRootFolder));
            return Create(applicationId, originalEntryPoint, fileReader);
        }

        public static ApplicationProxyReader Create(string applicationId, IAppxFileReader fileReader)
        {
            return Create(applicationId, null, fileReader);
        }
    }
}
