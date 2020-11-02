using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;
using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;

namespace Otor.MsixHero.Appx.Updates
{
    public interface IAppxBlockReader
    {
        Task<IList<Block>> ReadBlocks(IAppxFileReader fileReader, CancellationToken cancellationToken = default);

        Task<IList<Block>> ReadBlocks(FileInfo file, CancellationToken cancellationToken = default);

        void SetBlocks(ICollection<Block> oldBlocks, ICollection<Block> newBlocks, SdkComparePackage comparedPackage);
    }
}
