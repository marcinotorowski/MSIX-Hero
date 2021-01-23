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
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Serialization;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;
using File = System.IO.File;

namespace Otor.MsixHero.Appx.Updates
{
    public class AppxBlockMapUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        private readonly MsixSdkWrapper sdkWrapper;
        private readonly IAppxBlockReader blockReader;

        public AppxBlockMapUpdateImpactAnalyzer(MsixSdkWrapper sdkWrapper, IAppxBlockReader blockReader)
        {
            this.sdkWrapper = sdkWrapper;
            this.blockReader = blockReader;
        }

        public async Task<UpdateImpactResult> Analyze(string appxBlockMapPath1, string appxBlockMapPath2, CancellationToken cancellationToken = default)
        {
            var file1Name = Path.GetFileName(appxBlockMapPath1);
            var file2Name = Path.GetFileName(appxBlockMapPath2);

            if (!string.Equals(file1Name, "AppxBlockMap.xml", StringComparison.OrdinalIgnoreCase) || !string.Equals(file2Name, "AppxBlockMap.xml", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("This analyzer can only compare two appx block maps.");
            }

            var tempFile = Path.Join(Path.GetTempPath(), "msix-hero-cp-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            var results = new UpdateImpactResult();
            try
            {
                await this.sdkWrapper.ComparePackages(appxBlockMapPath1, appxBlockMapPath2, tempFile, cancellationToken).ConfigureAwait(false);
                var serializer = new ComparePackageSerializer();
                results.Comparison = serializer.Deserialize(tempFile);
                results.OldPackage = new UpdateImpactPackage { Path = appxBlockMapPath1 };
                results.NewPackage = new UpdateImpactPackage { Path = appxBlockMapPath2 };

                results.OldPackage.Blocks = await this.blockReader.ReadBlocks(new FileInfo(appxBlockMapPath1), cancellationToken).ConfigureAwait(false);
                results.NewPackage.Blocks = await this.blockReader.ReadBlocks(new FileInfo(appxBlockMapPath2), cancellationToken).ConfigureAwait(false);
                this.blockReader.SetBlocks(results.OldPackage.Blocks, results.NewPackage.Blocks, results.Comparison);

                return results;
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}