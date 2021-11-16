using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.SizeCalculator
{
    public class CimSizeCalculator : ISizeCalculator
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CimSizeCalculator));

        public Task<long> GetRequiredSize(string sourcePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            Logger.Debug($"Determining required size for CIM volume {sourcePath} with extra margin {(int)(100 * extraMargin)}%...");
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath), "Package path must not be empty.");
            }

            long total = 0;

            using (var archive = ZipFile.OpenRead(sourcePath))
            {
                foreach (var item in archive.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    total += item.Length;
                }
            }

            var actualMinSize = (long)(total * (1 + extraMargin));
            Logger.Info("Required minimum size for VHD volume is " + actualMinSize + " bytes.");
            return Task.FromResult(actualMinSize);
        }
    }
}
