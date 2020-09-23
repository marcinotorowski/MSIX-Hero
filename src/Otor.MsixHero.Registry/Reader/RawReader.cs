using System;
using System.IO;
using System.Threading.Tasks;

namespace Otor.MsixHero.Registry.Reader
{
    public class RawReader
    {
        public async Task<IRegHive> Create(string path)
        {
            using (var embedded = typeof(RawReader).Assembly.GetManifestResourceStream("Otor.MsixHero.Registry.Resources.empty.dat"))
            {
                using (var fs = File.OpenWrite(path))
                {
                    if (embedded == null)
                    {
                        throw new NotSupportedException();
                    }

                    await embedded.CopyToAsync(fs).ConfigureAwait(false);
                }
            }

            return new OffregRegistryHive(OffregLib.OffregHive.Open(path));
        }

        public Task<IRegHive> Create()
        {
            IRegHive hive = new OffregRegistryHive(OffregLib.OffregHive.Create());
            return Task.FromResult(hive);
        }
    }
}