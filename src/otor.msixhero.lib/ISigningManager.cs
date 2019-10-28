using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib
{
    public interface IAppxSigningManager
    {
        Task<bool> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory,
            string publisherName,
            string publisherDisplayName,
            string password,
            CancellationToken cancellationToken = default,
            IProgress<Progress.ProgressData> progress = null);
    }
}
