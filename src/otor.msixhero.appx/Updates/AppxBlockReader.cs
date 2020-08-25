using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;
using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;
using File = System.IO.File;

namespace Otor.MsixHero.Appx.Updates
{
    public class AppxBlockReader : IAppxBlockReader
    {
        public async Task<IList<Block>> ReadBlocks(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            if (!fileReader.FileExists("AppxBlockMap.xml"))
            {
                throw new ArgumentException("Could not read the source block definition.");
            }

            using (var file = fileReader.GetFile("AppxBlockMap.xml"))
            {
                return await this.ReadBlocks(file, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<IList<Block>> ReadBlocks(FileInfo file, CancellationToken cancellationToken = default)
        {
            if (!file.Exists)
            {
                throw new ArgumentException("Could not read the source block definition.");
            }

            using (var fileStream = File.OpenRead(file.FullName))
            {
                return await this.ReadBlocks(fileStream, cancellationToken).ConfigureAwait(false);
            }
        }

        public void SetBlocks(ICollection<Block> oldBlocks, ICollection<Block> newBlocks, SdkComparePackage comparedPackage)
        {
            var changedFileNames = new HashSet<string>(comparedPackage.Package?.ChangedFiles?.Items?.Select(f => f.Name) ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            var deletedFileNames = new HashSet<string>(comparedPackage.Package?.DeletedFiles?.Items?.Select(f => f.Name) ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            var addedFileNames = new HashSet<string>(comparedPackage.Package?.AddedFiles?.Items?.Select(f => f.Name) ?? Enumerable.Empty<string>(), StringComparer.Ordinal);

            foreach (var item in oldBlocks)
            {
                if (deletedFileNames.Contains(item.FileName))
                {
                    item.BlockType = BlockType.Deleted;
                }
                else if (changedFileNames.Contains(item.FileName))
                {
                    if (newBlocks.Any(b => b.FileName == item.FileName && b.Hash != item.Hash))
                    {
                        item.BlockType = BlockType.Changed;
                    }
                }
            }

            foreach (var item in newBlocks)
            {
                if (addedFileNames.Contains(item.FileName))
                {
                    item.BlockType = BlockType.Added;
                }
                else if (changedFileNames.Contains(item.FileName))
                {
                    if (oldBlocks.Any(b => b.FileName == item.FileName && b.Hash != item.Hash))
                    {
                        item.BlockType = BlockType.Changed;
                    }
                }
            }
        }

        private async Task<IList<Block>> ReadBlocks(Stream file, CancellationToken cancellationToken = default)
        {
            IList<Block> list = new List<Block>();
            var document = await XDocument.LoadAsync(file, LoadOptions.None, cancellationToken).ConfigureAwait(false);
            var blockMap = document.Root;
            if (blockMap == null || blockMap.Name.LocalName != "BlockMap")
            {
                return list;
            }

            var offset = 0L;

            var ns = XNamespace.Get("http://schemas.microsoft.com/appx/2010/blockmap");

            foreach (var item in blockMap.Elements(ns + "File"))
            {
                long.TryParse(item.Attribute("Size")?.Value ?? "0", out var fileSize);

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
                        long.TryParse(blockSize.Value ?? "0", out blockLength);
                    }


                    var fileName = item.Attribute("Name")?.Value;
                    var block = new Block
                    {
                        BlockType = BlockType.Unchanged,
                        FileName = fileName,
                        Hash = fileBlock.Attribute("Hash")?.Value,
                        Length = blockLength,
                        Position = offset
                    };

                    offset += block.Length;
                    list.Add(block);
                }
            }

            return list;
        }
    }
}