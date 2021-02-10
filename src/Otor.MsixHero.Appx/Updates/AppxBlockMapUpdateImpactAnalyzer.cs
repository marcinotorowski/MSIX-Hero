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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Updates.Entities;

namespace Otor.MsixHero.Appx.Updates
{
    public class AppxBlockMapUpdateImpactAnalyzer : IAppxUpdateImpactAnalyzer
    {
        public async Task<ComparisonResult> Analyze(string file1, string file2, CancellationToken cancellationToken = default)
        {
            await using (var stream1 = File.OpenRead(file1))
            {
                await using (var stream2 = File.OpenRead(file2))
                {
                    return await this.Analyze(stream1, stream2, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public async Task<ComparisonResult> Analyze(Stream appxBlockMap1, Stream appxBlockMap2, CancellationToken cancellationToken = default)
        {
            var files1 = await this.GetFiles(appxBlockMap1, cancellationToken).ConfigureAwait(false);
            var files2 = await this.GetFiles(appxBlockMap2, cancellationToken).ConfigureAwait(false);
            
            var filesInPackage1 = new SortedDictionary<string, ComparedFile>();
            var filesInPackage2 = new SortedDictionary<string, ComparedFile>();
            var blocksInPackage1 = new Dictionary<string, ComparedBlock>();
            var blocksInPackage2 = new Dictionary<string, ComparedBlock>();

            foreach (var file1 in files1)
            {
                foreach (var block in file1.Blocks)
                {
                    blocksInPackage1[block.Hash] = block;
                }

                filesInPackage1.Add(file1.Name, file1);
            }
            
            foreach (var file2 in files2)
            {
                foreach (var block in file2.Blocks)
                {
                    blocksInPackage2[block.Hash] = block;
                }

                filesInPackage2.Add(file2.Name, file2);
            }
            
            foreach (var file in filesInPackage1)
            {
                var declaredSize = file.Value.UncompressedSize;
                var compressed = file.Value.Blocks.Sum(b => b.CompressedSize);
                
                cancellationToken.ThrowIfCancellationRequested();
                if (!filesInPackage2.TryGetValue(file.Key, out var fileFromPackage2))
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

            foreach (var file in filesInPackage2)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!filesInPackage1.TryGetValue(file.Key, out _))
                {
                    file.Value.Status = ComparisonStatus.New;
                    file.Value.UpdateImpact = file.Value.Blocks.Sum(b => b.CompressedSize); // sum of all blocks
                    file.Value.SizeDifference = file.Value.UncompressedSize;
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

            var duplicates = new Dictionary<string, IList<ComparedFile>>();

            using (var md5 = MD5.Create())
            {
                foreach (var file in filesInPackage2.Values)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var hash = string.Join(System.Environment.NewLine, file.Blocks.Select(b => b.Hash));
                    var allBlocksHash = System.Text.Encoding.ASCII.GetString(md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(hash)));

                    if (!duplicates.TryGetValue(allBlocksHash, out var list))
                    {
                        list = new List<ComparedFile>();
                        duplicates[allBlocksHash] = list;
                    }

                    list.Add(file);
                }
            }

            var duplicatedFiles = new ComparedDuplicateFiles
            {
                Duplicates = new List<ComparedDuplicate>()
            };

            foreach (var file in duplicates.Where(d => d.Value.Count > 1))
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
            }

            cancellationToken.ThrowIfCancellationRequested();
            var changedFiles = new ComparedFiles
            {
                UpdateImpact = filesInPackage2.Values.Where(b => b.Status == ComparisonStatus.Changed).SelectMany(b => b.Blocks).Sum(b => b.UpdateImpact),
                ActualUpdateImpact = blocksInPackage2.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UpdateImpact),
                SizeDifference = filesInPackage2.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.SizeDifference),
                BaseTotalSize = filesInPackage1.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UncompressedSize),
                TargetTotalSize = filesInPackage2.Values.Where(b => b.Status == ComparisonStatus.Changed).Sum(b => b.UncompressedSize),
                Files = filesInPackage1.Values.Where(f => f.Status == ComparisonStatus.Changed).ToList()
            };

            cancellationToken.ThrowIfCancellationRequested();
            var addedFiles = new ComparedFiles
            {
                UpdateImpact = filesInPackage2.Where(b => b.Value.Status == ComparisonStatus.New).SelectMany(b => b.Value.Blocks).Sum(b => b.UpdateImpact),
                ActualUpdateImpact = blocksInPackage2.Where(b => b.Value.Status == ComparisonStatus.New).Sum(b => b.Value.UpdateImpact),
                BaseTotalSize = 0,
                TargetTotalSize = filesInPackage2.Values.Where(s => s.Status == ComparisonStatus.New).Sum(f => f.SizeDifference),
                Files = filesInPackage2.Values.Where(f => f.Status == ComparisonStatus.Changed).ToList()
            };

            addedFiles.SizeDifference = addedFiles.TargetTotalSize;

            cancellationToken.ThrowIfCancellationRequested();
            var deletedFiles = new ComparedFiles
            {
                BaseTotalSize = -1 * filesInPackage1.Values.Where(s => s.Status == ComparisonStatus.Old).Sum(f => f.SizeDifference),
                TargetTotalSize = 0,
                ActualUpdateImpact = blocksInPackage1.Values.Where(s => s.Status == ComparisonStatus.Old).Sum(f => f.UpdateImpact),
                Files = filesInPackage1.Values.Where(f => f.Status == ComparisonStatus.Old).ToList()
            };

            deletedFiles.UpdateImpact = deletedFiles.ActualUpdateImpact;
            deletedFiles.SizeDifference = -1 * deletedFiles.BaseTotalSize;

            cancellationToken.ThrowIfCancellationRequested();
            var unchangedFiles = new ComparedFiles
            {
                TargetTotalSize = filesInPackage2.Where(b => b.Value.Status == ComparisonStatus.Unchanged).Sum(b => b.Value.UncompressedSize),
                UpdateImpact = 0,
                SizeDifference = 0,
                ActualUpdateImpact = 0,
                Files = filesInPackage1.Values.Where(f => f.Status == ComparisonStatus.Unchanged).ToList()
            };

            unchangedFiles.BaseTotalSize = unchangedFiles.TargetTotalSize;

            cancellationToken.ThrowIfCancellationRequested();
            var comparisonResult = new ComparisonResult
            {
                //BaseTotalSize = filesInPackage1.Sum(f => f.Value.UncompressedSize),
                //TargetTotalSize = filesInPackage2.Sum(f => f.Value.UncompressedSize),
                BaseTotalCompressedSize = blocksInPackage1.Sum(f => f.Value.CompressedSize),
                TargetTotalCompressedSize = blocksInPackage2.Sum(f => f.Value.CompressedSize),
                UpdateImpact =
                    blocksInPackage2.Where(b => b.Value.Status == ComparisonStatus.New).Sum(b => b.Value.UpdateImpact) +
                    blocksInPackage1.Sum(b => b.Value.UpdateImpact),
                ChangedFiles = changedFiles,
                DeletedFiles = deletedFiles,
                DuplicateFiles = duplicatedFiles,
                NewFiles = addedFiles,
                UnchangedFiles = unchangedFiles,
                BaseChart = new ComparisonChart
                {
                    Blocks = this.GetChartForBlocks(filesInPackage1),
                    Files = this.GetChartForFiles(filesInPackage1)
                },
                TargetChart = new ComparisonChart
                {
                    Blocks = this.GetChartForBlocks(filesInPackage2),
                    Files = this.GetChartForFiles(filesInPackage2)
                }
            };

            comparisonResult.BaseTotalSize = comparisonResult.DeletedFiles.BaseTotalSize + comparisonResult.UnchangedFiles.BaseTotalSize + comparisonResult.ChangedFiles.BaseTotalSize;
            comparisonResult.TargetTotalSize = comparisonResult.NewFiles.TargetTotalSize + comparisonResult.UnchangedFiles.TargetTotalSize + comparisonResult.ChangedFiles.TargetTotalSize;

            comparisonResult.SizeDifference = comparisonResult.TargetTotalSize - comparisonResult.BaseTotalSize;
            comparisonResult.ActualUpdateImpact = comparisonResult.UpdateImpact;

            return comparisonResult;
        }

        private async Task<IList<ComparedFile>> GetFiles(Stream appxBlockMap, CancellationToken cancellationToken)
        {
            IList<ComparedFile> list = new List<ComparedFile>();
            var document = await XDocument.LoadAsync(appxBlockMap, LoadOptions.None, cancellationToken);
            var blockMap = document.Root;
            if (blockMap == null || blockMap.Name.LocalName != "BlockMap")
            {
                return list;
            }

            var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2010/blockmap");

            foreach (var item in blockMap.Elements(ns + "File"))
            {
                long.TryParse(item.Attribute("Size")?.Value ?? "0", out var fileSize);

                var fileName = item.Attribute("Name")?.Value;
                var file = new ComparedFile(fileName, fileSize);

                foreach (var fileBlock in item.Elements(ns + "Block"))
                {
                    long blockLength;
                    var blockSize = fileBlock.Attribute("Size");
                    if (blockSize == null)
                    {
                        blockLength = fileSize;
                    }
                    else
                    {
                        long.TryParse(blockSize.Value, out blockLength);
                    }

                    file.Blocks.Add(new ComparedBlock(fileBlock.Attribute("Hash")?.Value, blockLength));
                }

                list.Add(file);
            }

            return list;
        }

        private IList<ComparedChart> GetChartForBlocks(SortedDictionary<string, ComparedFile> files)
        {
            var spaces = new List<ComparedChart>();

            ComparedChart previousBlock = null;
            long position = 0;

            var processed = new HashSet<string>();

            foreach (var block in files.SelectMany(f => f.Value.Blocks))
            {
                var currentStatus = block.Status;
                if (!processed.Add(block.Hash))
                {
                    currentStatus = ComparisonStatus.Duplicate;
                }

                if (previousBlock == null)
                {
                    previousBlock = new ComparedChart
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
                    previousBlock = new ComparedChart
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

        private IList<ComparedChart> GetChartForFiles(SortedDictionary<string, ComparedFile> files)
        {
            var spaces = new List<ComparedChart>();

            ComparedChart previousBlock = null;
            long position = 0;
            foreach (var file in files)
            {
                if (previousBlock == null)
                {
                    previousBlock = new ComparedChart
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
                    previousBlock = new ComparedChart
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