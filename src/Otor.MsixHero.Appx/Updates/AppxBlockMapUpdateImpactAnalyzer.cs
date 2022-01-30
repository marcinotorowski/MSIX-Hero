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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Entities.Appx;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Updates
{
    /// <summary>
    /// A class that compares two AppxBlockMap.xml files and returns the comparison result.
    /// </summary>
    /// <remarks>A lot of code inside this class may be easily optimized (for example a lot of LINQ queries keep
    /// going back and forth the same collections). Since the performance of this class is not critical, and the
    /// actual execution takes just a fraction of second even for big packages, having a clear functional programming
    /// instead of too much optimization is a clear win.</remarks>
    public class AppxBlockMapUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public async Task<UpdateImpactResults> Analyze(
            string file1,
            string file2,
            bool ignoreVersionCheck = false,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressReporter = default)
        {
            await using var stream1 = File.OpenRead(file1);
            await using var stream2 = File.OpenRead(file2);
            var result = await this.Analyze(stream1, stream2, cancellationToken, progressReporter).ConfigureAwait(false);
            result.OldPackage = file1;
            result.NewPackage = file2;
            return result;
        }

        public async Task<UpdateImpactResults> Analyze(
            Stream appxBlockMap1,
            Stream appxBlockMap2,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progressData = default)
        {
            var progressReadFiles1 = new RangeProgress(progressData, 0, 30);
            var progressReadFiles2 = new RangeProgress(progressData, 30, 60);
            var progressCalculatingStatus = new RangeProgress(progressData, 60, 75);
            var progressDuplicates = new RangeProgress(progressData, 75, 90);
            var progressRemainingCalculation = new RangeProgress(progressData, 90, 100);
            
            var filesInOldPackage = new SortedDictionary<string, AppxFile>();
            var filesInNewPackage = new SortedDictionary<string, AppxFile>();
            var blocksInPackage1 = new Dictionary<string, AppxBlock>();
            var blocksInPackage2 = new Dictionary<string, AppxBlock>();

            progressReadFiles1.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_Reading1));
            var files1 = await this.GetFiles(appxBlockMap1, cancellationToken, progressReadFiles1).ConfigureAwait(false);
            foreach (var file1 in files1)
            {
                foreach (var block in file1.Blocks)
                {
                    blocksInPackage1[block.Hash] = block;
                }

                filesInOldPackage.Add(file1.Name, file1);
            }

            progressReadFiles2.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_Reading2));
            var files2 = await this.GetFiles(appxBlockMap2, cancellationToken, progressReadFiles2).ConfigureAwait(false);
            foreach (var file2 in files2)
            {
                foreach (var block in file2.Blocks)
                {
                    blocksInPackage2[block.Hash] = block;
                }

                filesInNewPackage.Add(file2.Name, file2);
            }

            progressCalculatingStatus.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_Calculating1));
            foreach (var file in filesInOldPackage)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!filesInNewPackage.TryGetValue(file.Key, out var fileFromPackage2))
                {
                    file.Value.Status = ComparisonStatus.Old;
                    file.Value.UpdateImpact = 0; // file removed = no update on impact
                    file.Value.SizeDifference = -file.Value.UncompressedSize;
                    continue;
                }

                if (file.Value.UncompressedSize == fileFromPackage2.UncompressedSize && file.Value.Blocks.Select(b => b.Hash).SequenceEqual(fileFromPackage2.Blocks.Select(b => b.Hash)))
                {
                    file.Value.Status = ComparisonStatus.Unchanged;
                    file.Value.UpdateImpact = 0; // file unchanged = no update on impact
                    file.Value.SizeDifference = 0;

                    fileFromPackage2.Status = ComparisonStatus.Unchanged;
                    fileFromPackage2.UpdateImpact = 0; // file unchanged = no update on impact
                    fileFromPackage2.SizeDifference = 0;
                }
                else
                {
                    file.Value.Status = ComparisonStatus.Changed;
                    file.Value.UpdateImpact = 0; // file changed = show no update impact. The impact should be shown in the other package.
                    file.Value.SizeDifference = file.Value.UncompressedSize - fileFromPackage2.UncompressedSize;

                    var blocksOnlyInPackage2 = fileFromPackage2.Blocks.Select(b => b.Hash).Except(file.Value.Blocks.Select(b => b.Hash));
                    fileFromPackage2.Status = ComparisonStatus.Changed;
                    fileFromPackage2.UpdateImpact = blocksOnlyInPackage2.Select(b => blocksInPackage2[b]).Sum(b => b.CompressedSize);
                    fileFromPackage2.SizeDifference = fileFromPackage2.UncompressedSize - file.Value.UncompressedSize;
                }
            }
            foreach (var block1KeyValuePair in blocksInPackage1)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var block1 = block1KeyValuePair.Value;
                if (!blocksInPackage2.TryGetValue(block1KeyValuePair.Key, out var block2))
                {
                    block1.Status = ComparisonStatus.Old;
                    block1.UpdateImpact = 0; // block removed, no update impact
                }
                else
                {
                    block1.Status = ComparisonStatus.Unchanged;
                    block1.UpdateImpact = 0;
                    block2.Status = ComparisonStatus.Unchanged;
                    block2.UpdateImpact = 0;
                }
            }
            
            progressCalculatingStatus.Report(new ProgressData(50, Resources.Localization.Packages_UpdateImpact_Calculating2));
            foreach (var file in filesInNewPackage)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!filesInOldPackage.TryGetValue(file.Key, out _))
                {
                    file.Value.Status = ComparisonStatus.New;
                    file.Value.UpdateImpact = file.Value.Blocks.Sum(b => b.CompressedSize); // sum of all blocks
                    file.Value.SizeDifference = file.Value.UncompressedSize;
                }
            }
            foreach (var block2KeyValuePair in blocksInPackage2)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var block2 = block2KeyValuePair.Value;
                if (!blocksInPackage1.ContainsKey(block2KeyValuePair.Key))
                {
                    block2.Status = ComparisonStatus.New;
                    block2.UpdateImpact = block2.CompressedSize;
                }
            }

            var duplicates1 = new Dictionary<string, IList<AppxFile>>();
            var duplicates2 = new Dictionary<string, IList<AppxFile>>();

            progressDuplicates.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_Deduplicating));
            using (var md5 = MD5.Create())
            {
                foreach (var file in filesInOldPackage.Values)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var hash = string.Join(Environment.NewLine, file.Blocks.Select(b => b.Hash));
                    var allBlocksHash = System.Text.Encoding.ASCII.GetString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hash)));

                    if (!duplicates1.TryGetValue(allBlocksHash, out var list))
                    {
                        list = new List<AppxFile>();
                        duplicates1[allBlocksHash] = list;
                    }

                    list.Add(file);
                }
                
                foreach (var file in filesInNewPackage.Values)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var hash = string.Join(System.Environment.NewLine, file.Blocks.Select(b => b.Hash));
                    var allBlocksHash = System.Text.Encoding.ASCII.GetString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hash)));

                    if (!duplicates2.TryGetValue(allBlocksHash, out var list))
                    {
                        list = new List<AppxFile>();
                        duplicates2[allBlocksHash] = list;
                    }

                    list.Add(file);
                }
            }

            var duplicatedFiles1 = new AppxDuplication
            {
                Duplicates = new List<ComparedDuplicate>(),
                FileCount = duplicates1.Count(d => d.Value.Count > 1) * 2,
                FileSize = duplicates1.Where(d => d.Value.Count > 1).Sum(d => d.Value[0].UncompressedSize)
            };

            var duplicatedFiles2 = new AppxDuplication
            {
                Duplicates = new List<ComparedDuplicate>(),
                FileCount = duplicates2.Count(d => d.Value.Count > 1) * 2,
                FileSize = duplicates2.Where(d => d.Value.Count > 1).Sum(d => d.Value[0].UncompressedSize)
            };

            progressDuplicates.Report(new ProgressData(55, Resources.Localization.Packages_UpdateImpact_DeduplicatingImpact1));
            foreach (var file in duplicates1.Where(d => d.Value.Count > 1))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var duplicate = new ComparedDuplicate
                {
                    Files = new List<ComparedDuplicateFile>()
                };

                foreach (var df in file.Value)
                {
                    var mdf = new ComparedDuplicateFile
                    {
                        Name = df.Name,
                        PossibleSizeReduction = df.UncompressedSize,
                        PossibleImpactReduction = df.Blocks.Sum(d => blocksInPackage1[d.Hash].CompressedSize)
                    };

                    duplicate.Files.Add(mdf);
                }

                duplicate.PossibleSizeReduction = duplicate.Files[0].PossibleSizeReduction * (duplicate.Files.Count - 1);
                duplicate.PossibleImpactReduction = duplicate.Files[0].PossibleImpactReduction * (duplicate.Files.Count - 1);

                duplicatedFiles1.Duplicates.Add(duplicate);
            }

            progressDuplicates.Report(new ProgressData(70, Resources.Localization.Packages_UpdateImpact_DeduplicatingImpact2));
            foreach (var file in duplicates2.Where(d => d.Value.Count > 1))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var duplicate = new ComparedDuplicate
                {
                    Files = new List<ComparedDuplicateFile>()
                };

                foreach (var df in file.Value)
                {
                    var mdf = new ComparedDuplicateFile
                    {
                        Name = df.Name,
                        PossibleSizeReduction = df.UncompressedSize,
                        PossibleImpactReduction = df.Blocks.Sum(d => blocksInPackage2[d.Hash].CompressedSize)
                    };

                    duplicate.Files.Add(mdf);
                }

                duplicate.PossibleSizeReduction = duplicate.Files[0].PossibleSizeReduction * (duplicate.Files.Count - 1);
                duplicate.PossibleImpactReduction = duplicate.Files[0].PossibleImpactReduction * (duplicate.Files.Count - 1);

                duplicatedFiles2.Duplicates.Add(duplicate);
            }

            progressDuplicates.Report(new ProgressData(85, Resources.Localization.Packages_UpdateImpact_DeduplicatingImpact3));
            duplicatedFiles1.PossibleImpactReduction = duplicatedFiles1.Duplicates.Sum(d => d.PossibleImpactReduction);
            duplicatedFiles1.PossibleSizeReduction = duplicatedFiles1.Duplicates.Sum(d => d.PossibleSizeReduction);
            duplicatedFiles2.PossibleImpactReduction = duplicatedFiles2.Duplicates.Sum(d => d.PossibleImpactReduction);
            duplicatedFiles2.PossibleSizeReduction = duplicatedFiles2.Duplicates.Sum(d => d.PossibleSizeReduction);
            
            cancellationToken.ThrowIfCancellationRequested();
            progressRemainingCalculation.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_AnalyzingChanges));
            var changedFiles = new ChangedFiles
            {
                // Shared parameters
                UpdateImpact = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).SelectMany(b => b.Blocks).Sum(b => b.UpdateImpact),
                ActualUpdateImpact = blocksInPackage2.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UpdateImpact),
                SizeDifference = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.SizeDifference),
                FileCount = filesInOldPackage.Values.Count(b => b.Status == ComparisonStatus.Changed),

                // Old package
                OldPackageFiles = filesInOldPackage.Values.Where(f => f.Status == ComparisonStatus.Changed).ToList(),
                OldPackageFileSize = filesInOldPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UncompressedSize),
                OldPackageBlockCount = filesInOldPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.Blocks.Count),
                OldPackageBlockSize = filesInOldPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).SelectMany(b => b.Blocks).Sum(b => b.CompressedSize),
                
                // New package
                NewPackageFiles = filesInNewPackage.Values.Where(f => f.Status == ComparisonStatus.Changed).ToList(),
                NewPackageFileSize = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UncompressedSize),
                NewPackageBlockCount = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.Blocks.Count),
                NewPackageBlockSize = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Changed).SelectMany(b => b.Blocks).Sum(b => b.CompressedSize)
            };

            cancellationToken.ThrowIfCancellationRequested();
            progressRemainingCalculation.Report(new ProgressData(25, Resources.Localization.Packages_UpdateImpact_AnalyzingAdded));
            var addedFiles = new AddedFiles
            {
                UpdateImpact = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.New).SelectMany(b => b.Blocks).Sum(b => b.UpdateImpact),
                ActualUpdateImpact = blocksInPackage2.Values.Where(b => b.Status == ComparisonStatus.New).Sum(b => b.UpdateImpact),
                BlockCount = blocksInPackage2.Values.Count(b => b.Status == ComparisonStatus.New),
                BlockSize = blocksInPackage2.Values.Where(b => b.Status == ComparisonStatus.New).Sum(b => b.CompressedSize),
                FileCount = filesInNewPackage.Values.Count(s => s.Status == ComparisonStatus.New),
                SizeDifference = filesInNewPackage.Values.Where(s => s.Status == ComparisonStatus.New).Sum(f => f.SizeDifference),
                FileSize = filesInNewPackage.Values.Where(s => s.Status == ComparisonStatus.New).Sum(f => f.UncompressedSize),
                Files = filesInNewPackage.Values.Where(f => f.Status == ComparisonStatus.New).ToList()
            };
            
            cancellationToken.ThrowIfCancellationRequested();
            progressRemainingCalculation.Report(new ProgressData(50, Resources.Localization.Packages_UpdateImpact_AnalyzingDeleted));
            var deletedFiles = new DeletedFiles
            {
                FileSize = filesInOldPackage.Values.Where(s => s.Status == ComparisonStatus.Old).Sum(f => f.UncompressedSize),
                SizeDifference = filesInOldPackage.Values.Where(s => s.Status == ComparisonStatus.Old).Sum(f => f.SizeDifference),
                FileCount = filesInOldPackage.Values.Count(s => s.Status == ComparisonStatus.Old),
                BlockSize = blocksInPackage1.Values.Where(b => b.Status == ComparisonStatus.Old).Sum(b => b.CompressedSize),
                BlockCount = blocksInPackage1.Values.Count(b => b.Status == ComparisonStatus.Old),
                ActualUpdateImpact = blocksInPackage1.Values.Where(s => s.Status == ComparisonStatus.Old).Sum(f => f.UpdateImpact),
                Files = filesInOldPackage.Values.Where(f => f.Status == ComparisonStatus.Old).ToList()
            };

            deletedFiles.UpdateImpact = deletedFiles.ActualUpdateImpact;

            cancellationToken.ThrowIfCancellationRequested();
            progressRemainingCalculation.Report(new ProgressData(75, Resources.Localization.Packages_UpdateImpact_AnalyzingUnchanged));
            var unchangedFiles = new UnchangedFiles
            {
                FileCount = filesInNewPackage.Values.Count(b => b.Status == ComparisonStatus.Unchanged),
                BlockCount = blocksInPackage2.Values.Count(b => b.Status == ComparisonStatus.Unchanged),
                BlockSize = blocksInPackage2.Values.Where(b => b.Status == ComparisonStatus.Unchanged).Sum(b => b.CompressedSize),
                FileSize = filesInNewPackage.Values.Where(b => b.Status == ComparisonStatus.Unchanged).Sum(b => b.UncompressedSize),
                Files = filesInOldPackage.Values.Where(f => f.Status == ComparisonStatus.Unchanged).ToList()
            };
            
            cancellationToken.ThrowIfCancellationRequested();
            progressRemainingCalculation.Report(new ProgressData(90, Resources.Localization.Packages_UpdateImpact_Wait));
            var comparisonResult = new UpdateImpactResults
            {
                OldPackageLayout = new AppxLayout
                {
                    FileSize = filesInOldPackage.Sum(f => f.Value.UncompressedSize),
                    FileCount = filesInOldPackage.Count,
                    BlockSize = blocksInPackage1.Sum(f => f.Value.CompressedSize),
                    BlockCount = blocksInPackage1.Count,
                    Layout = new PackageLayout
                    {
                        Blocks = this.GetChartForBlocks(filesInOldPackage),
                        Files = this.GetChartForFiles(filesInOldPackage)
                    }
                },
                NewPackageLayout = new AppxLayout
                {
                    FileSize = filesInNewPackage.Sum(f => f.Value.UncompressedSize),
                    FileCount = filesInNewPackage.Count,
                    BlockSize = blocksInPackage2.Sum(f => f.Value.CompressedSize),
                    BlockCount = blocksInPackage2.Count,
                    Layout = new PackageLayout
                    {
                        Blocks = this.GetChartForBlocks(filesInNewPackage),
                        Files = this.GetChartForFiles(filesInNewPackage)
                    }
                },
                OldPackageDuplication = duplicatedFiles1,
                NewPackageDuplication = duplicatedFiles2,
                UpdateImpact =
                    blocksInPackage2.Where(b => b.Value.Status == ComparisonStatus.New).Sum(b => b.Value.UpdateImpact) +
                    blocksInPackage1.Sum(b => b.Value.UpdateImpact),
                ChangedFiles = changedFiles,
                DeletedFiles = deletedFiles,
                AddedFiles = addedFiles,
                UnchangedFiles = unchangedFiles
            };

            comparisonResult.OldPackageLayout.Size = comparisonResult.OldPackageLayout.BlockSize + filesInOldPackage.Values.Sum(f => f.HeaderSize);
            comparisonResult.NewPackageLayout.Size = comparisonResult.NewPackageLayout.BlockSize + filesInNewPackage.Values.Sum(f => f.HeaderSize);
            
            comparisonResult.SizeDifference = comparisonResult.AddedFiles.SizeDifference + comparisonResult.ChangedFiles.SizeDifference + comparisonResult.DeletedFiles.SizeDifference;
            comparisonResult.ActualUpdateImpact = comparisonResult.UpdateImpact;
            
            return comparisonResult;
        }

        private async Task<IList<AppxFile>> GetFiles(Stream appxBlockMap, CancellationToken cancellationToken, IProgress<ProgressData> progress = null)
        {
            progress?.Report(new ProgressData(0, Resources.Localization.Packages_UpdateImpact_ReadingBlock));
            IList<AppxFile> list = new List<AppxFile>();
            var document = await XDocument.LoadAsync(appxBlockMap, LoadOptions.None, cancellationToken);
            var blockMap = document.Root;
            if (blockMap == null || blockMap.Name.LocalName != "BlockMap")
            {
                return list;
            }

            var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2010/blockmap");

            const int maxBlockSize = 64 * 1024; // 64 kilobytes

            progress?.Report(new ProgressData(50, Resources.Localization.Packages_UpdateImpact_ReadingFiles));
            foreach (var item in blockMap.Elements(ns + "File"))
            {
                long.TryParse(item.Attribute("Size")?.Value ?? "0", out var fileSize);

                ushort headerLength;
                var headerSize = item.Attribute("LfhSize");
                if (headerSize == null)
                {
                    headerLength = 30;
                }
                else
                {
                    ushort.TryParse(headerSize.Value, out headerLength);
                }

                var fileName = item.Attribute("Name")?.Value;
                var file = new AppxFile(fileName, fileSize, headerLength);
                
                foreach (var fileBlock in item.Elements(ns + "Block"))
                {
                    long blockLength;
                    var blockSize = fileBlock.Attribute("Size");
                    if (blockSize == null)
                    {
                        // If this is null, the block is uncompressed.
                        if (fileSize > maxBlockSize)
                        {
                            blockLength = maxBlockSize;
                        }
                        else
                        {
                            blockLength = fileSize;
                        }

                        fileSize -= maxBlockSize;
                    }
                    else
                    {
                        long.TryParse(blockSize.Value, out blockLength);
                    }
                    
                    file.Blocks.Add(new AppxBlock(fileBlock.Attribute("Hash")?.Value, blockLength) { Status = ComparisonStatus.Unchanged });
                }

                list.Add(file);
            }

            return list;
        }

        private List<LayoutBar> GetChartForBlocks(SortedDictionary<string, AppxFile> files)
        {
            var spaces = new List<LayoutBar>();

            LayoutBar previousBlock = null;
            long position = 0;

            var processed = new HashSet<string>();

            foreach (var block in files.SelectMany(f => f.Value.Blocks))
            {
                var currentStatus = block.Status;
                if (!processed.Add(block.Hash))
                {
                    // todo: investigate
                    // currentStatus = ComparisonStatus.Duplicate;
                }

                if (previousBlock == null)
                {
                    previousBlock = new LayoutBar
                    {
                        Status = currentStatus,
                        Position = position,
                        Size = block.CompressedSize
                    };
                }
                else if (previousBlock.Status == currentStatus)
                {
                    previousBlock.Size += block.CompressedSize;
                }
                else
                {
                    spaces.Add(previousBlock);
                    previousBlock = new LayoutBar
                    {
                        Status = currentStatus,
                        Position = position,
                        Size = block.CompressedSize
                    };
                }

                position += block.CompressedSize;
            }

            if (previousBlock != null)
            {
                spaces.Add(previousBlock);
            }

            return spaces;
        }

        private List<LayoutBar> GetChartForFiles(SortedDictionary<string, AppxFile> files)
        {
            var spaces = new List<LayoutBar>();

            LayoutBar previousBlock = null;
            long position = 0;
            foreach (var file in files)
            {
                if (previousBlock == null)
                {
                    previousBlock = new LayoutBar
                    {
                        Status = file.Value.Status,
                        Position = position,
                        Size = file.Value.UncompressedSize
                    };
                }
                else if (previousBlock.Status == file.Value.Status)
                {
                    previousBlock.Size += file.Value.UncompressedSize;
                }
                else
                {
                    spaces.Add(previousBlock);
                    previousBlock = new LayoutBar
                    {
                        Status = file.Value.Status,
                        Position = position,
                        Size = file.Value.UncompressedSize
                    };
                }

                position += file.Value.UncompressedSize;
            }

            if (previousBlock != null)
            {
                spaces.Add(previousBlock);
            }

            return spaces;
        }
    }
}