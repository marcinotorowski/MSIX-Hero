using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Packaging.ManifestCreator
{
    public interface IAppxManifestCreator
    {
        Task<IList<CreatedItem>> CreateManifestForDirectory(
            DirectoryInfo sourceDirectory, 
            AppxManifestCreatorOptions options = default, 
            CancellationToken cancellationToken = default, 
            IProgress<Progress> progress = null);

        Task<AppxManifestCreatorAdviser> AnalyzeDirectory(DirectoryInfo directoryInfo, CancellationToken cancellationToken = default);
    }
}
