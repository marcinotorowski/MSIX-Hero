using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach.MountVol
{
    public static class MountVolumeHelper
    {
        public static async Task<IList<Guid>> GetVolumeIdentifiers()
        {
            var psi = new ProcessStartInfo("mountvol") { CreateNoWindow = true, RedirectStandardOutput = true };
            // ReSharper disable once PossibleNullReferenceException
            var output = Process.Start(psi).StandardOutput;
            var allVolumes = await output.ReadToEndAsync().ConfigureAwait(false);

            var list = new List<Guid>();
            foreach (var item in allVolumes.Split(Environment.NewLine))
            {
                var io = item.IndexOf(@"\\?\Volume{", StringComparison.OrdinalIgnoreCase);
                if (io == -1)
                {
                    continue;
                }

                io -= 1;

                var guidString = item.Substring(io + @"\\?\Volume{".Length);
                var closing = guidString.IndexOf('}');
                if (closing == -1)
                {
                    continue;
                }

                guidString = guidString.Substring(0, closing + 1);
                if (Guid.TryParse(guidString, out var parsed))
                {
                    list.Add(parsed);
                }
            }

            return list;
        }
    }
}
