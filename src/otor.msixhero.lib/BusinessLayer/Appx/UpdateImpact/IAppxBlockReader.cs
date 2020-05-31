using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.Domain.Appx.UpdateImpact.Blocks;
using otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage;

namespace otor.msixhero.lib.BusinessLayer.Appx.UpdateImpact
{
    public interface IAppxBlockReader
    {
        Task<IList<Block>> ReadBlocks(IAppxFileReader fileReader, CancellationToken cancellationToken = default);

        Task<IList<Block>> ReadBlocks(FileInfo file, CancellationToken cancellationToken = default);

        void SetBlocks(ICollection<Block> oldBlocks, ICollection<Block> newBlocks, SdkComparePackage comparedPackage);
    }
}
