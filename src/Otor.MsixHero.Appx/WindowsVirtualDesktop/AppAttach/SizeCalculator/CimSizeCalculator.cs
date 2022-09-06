using System;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.SizeCalculator
{
    public class CimSizeCalculator : ISizeCalculator
    {
        private static readonly LogSource Logger = new();
        public Task<uint> GetRequiredSize(string sourcePath, double extraMargin = 0.2, CancellationToken cancellationToken = default)
        {
            Logger.Debug().WriteLine($"Determining required size for CIM volume {sourcePath} with extra margin {(int)(100 * extraMargin)}%…");
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath), Resources.Localization.Packages_Error_EmptyPath);
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
            Logger.Info().WriteLine("Required minimum size for VHD volume is " + actualMinSize + " bytes.");
            return Task.FromResult((uint)(Math.Floor(actualMinSize / 1024.0 / 1024.0)));
        }
    }
}
